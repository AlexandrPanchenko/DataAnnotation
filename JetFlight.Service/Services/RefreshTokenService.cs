using System.Data;
using System.Security.Cryptography;
using System.Text;
using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Shared.Models.Store;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace JetFlight.Service.Services
{
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Generates a new refresh token for a customer.
        /// </summary>
        Task<(string token, RefreshToken refreshTokenEntity)> GenerateRefreshTokenAsync(
            int customerId,
            byte branchId,
            string? ipAddress,
            string? userAgent,
            Guid? tokenFamily = null);

        /// <summary>
        /// Rotates a refresh token: validates old token, generates new token, revokes old token.
        /// </summary>
        Task<(string newToken, RefreshToken newRefreshTokenEntity, Customer customer)?> RotateRefreshTokenAsync(
            string oldToken,
            string? ipAddress,
            string? userAgent);

        /// <summary>
        /// Revokes a refresh token.
        /// </summary>
        Task<bool> RevokeRefreshTokenAsync(string token, string reason);

        /// <summary>
        /// Revokes all refresh tokens for a customer.
        /// </summary>
        Task RevokeAllCustomerTokensAsync(int customerId, string reason);

        /// <summary>
        /// Revokes all tokens in a token family (security measure for token reuse detection).
        /// </summary>
        Task RevokeTokenFamilyAsync(Guid tokenFamily, string reason);

        /// <summary>
        /// Cleans up expired and revoked tokens older than specified days.
        /// </summary>
        Task<int> CleanupExpiredTokensAsync(int olderThanDays = 60);
    }

    public class RefreshTokenService : IRefreshTokenService
    {
        private const int TokenExpirationDays = 365; // Користувач залишається в системі до явного розлогінення
        private const int TokenByteLength = 32; // 256 bits
        private const int GracePeriodSeconds = 20;
        private const string RotationReason = "Token rotated";

        private const string RotationCacheKeyPrefix = "refresh-token-rotation:";

        private readonly IntegrationDataContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogger<RefreshTokenService> _logger;

        public RefreshTokenService(
            IntegrationDataContext context,
            IDistributedCache cache,
            ILogger<RefreshTokenService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<(string token, RefreshToken refreshTokenEntity)> GenerateRefreshTokenAsync(
            int customerId,
            byte branchId,
            string? ipAddress,
            string? userAgent,
            Guid? tokenFamily = null)
        {
            var (token, refreshToken) = CreateRefreshToken(
                customerId,
                branchId,
                ipAddress,
                userAgent,
                tokenFamily);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync(CancellationToken.None, ignoreLogs: true);

            return (token, refreshToken);
        }

        public async Task<(string newToken, RefreshToken newRefreshTokenEntity, Customer customer)?> RotateRefreshTokenAsync(
            string oldToken,
            string? ipAddress,
            string? userAgent)
        {
            var oldTokenHash = ComputeSha256Hash(oldToken);
            var cacheKey = BuildRotationCacheKey(oldToken);

            // Use Serializable isolation to handle concurrent requests
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                // Load token with transaction lock (Serializable isolation provides implicit locking)
                var oldRefreshToken = await _context.RefreshTokens
                    .Include(rt => rt.Customer)
                    .FirstOrDefaultAsync(rt => rt.TokenHash == oldTokenHash);

                if (oldRefreshToken == null)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                var now = DateTime.UtcNow;

                // Check if token is already revoked
                if (oldRefreshToken.IsRevoked)
                {
                    await transaction.RollbackAsync();

                    // Check if this was a rotation within the grace period (concurrent request)
                    if (string.Equals(oldRefreshToken.RevokedReason, RotationReason, StringComparison.OrdinalIgnoreCase) &&
                        oldRefreshToken.ReplacedAt.HasValue &&
                        oldRefreshToken.ReplacedAt.Value.AddSeconds(GracePeriodSeconds) > now)
                    {
                        // Within grace period - this is a concurrent request, not an attack
                        _logger.LogInformation(
                            "Token already rotated within grace period for family {TokenFamily}. Likely concurrent request.",
                            oldRefreshToken.TokenFamily);


                        var cachedToken = await _cache.GetStringAsync(cacheKey);

                        if (!string.IsNullOrEmpty(cachedToken) && !string.IsNullOrEmpty(oldRefreshToken.ReplacedByTokenHash))
                        {
                            var replacementToken = await _context.RefreshTokens
                                .FirstOrDefaultAsync(rt => rt.TokenHash == oldRefreshToken.ReplacedByTokenHash);

                            if (replacementToken != null)
                            {
                                replacementToken.Customer = oldRefreshToken.Customer;
                                return (cachedToken, replacementToken, oldRefreshToken.Customer);
                            }
                        }

                        return null;
                    }

                    // Outside grace period or different revocation reason - treat as attack
                    _logger.LogWarning(
                        "Token reuse attack detected for family {TokenFamily}. " +
                        "Token was revoked at {RevokedAt}, current time {Now}. Revoking entire family.",
                        oldRefreshToken.TokenFamily,
                        oldRefreshToken.RevokedAt,
                        now);

                    await RevokeTokenFamilyAsync(oldRefreshToken.TokenFamily, "Token reuse detected - security breach");
                    return null;
                }

                // Validate token state
                if (oldRefreshToken.ExpiresAt < now)
                {
                    await transaction.RollbackAsync();
                    _logger.LogInformation("Expired token used in rotation attempt");
                    return null;
                }

                if (oldRefreshToken.Customer.IsBlocked || oldRefreshToken.Customer.IsDeleted)
                {
                    await transaction.RollbackAsync();
                    _logger.LogInformation("Blocked or deleted customer attempted token rotation");
                    return null;
                }

                // Create new token and revoke old one
                var (newToken, newRefreshTokenEntity) = CreateRefreshToken(
                    oldRefreshToken.CustomerId,
                    oldRefreshToken.BranchId,
                    ipAddress,
                    userAgent,
                    oldRefreshToken.TokenFamily);

                oldRefreshToken.IsRevoked = true;
                oldRefreshToken.RevokedAt = now;
                oldRefreshToken.ReplacedAt = now;
                oldRefreshToken.RevokedReason = RotationReason;
                oldRefreshToken.ReplacedByTokenHash = newRefreshTokenEntity.TokenHash;

                _context.RefreshTokens.Add(newRefreshTokenEntity);

                await _context.SaveChangesAsync(CancellationToken.None, ignoreLogs: true);
                await transaction.CommitAsync();

                newRefreshTokenEntity.Customer = oldRefreshToken.Customer;

                var cacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(GracePeriodSeconds)
                };

                await _cache.SetStringAsync(cacheKey, newToken, cacheEntryOptions);

                _logger.LogInformation(
                    "Token rotation completed successfully for family {TokenFamily}",
                    oldRefreshToken.TokenFamily);

                return (newToken, newRefreshTokenEntity, oldRefreshToken.Customer);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Log serialization failures (concurrent requests)
                if (ex is DbUpdateException || ex.InnerException is SqlException)
                {
                    _logger.LogInformation(ex, "Concurrent token rotation detected, transaction rolled back");
                }
                else
                {
                    _logger.LogError(ex, "Error during token rotation");
                }

                throw;
            }
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token, string reason)
        {
            var tokenHash = ComputeSha256Hash(token);

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            if (refreshToken == null || refreshToken.IsRevoked)
            {
                return false;
            }

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedReason = TruncateReason(reason);

            await _context.SaveChangesAsync(CancellationToken.None, ignoreLogs: true);

            return true;
        }

        public async Task RevokeAllCustomerTokensAsync(int customerId, string reason)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.CustomerId == customerId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = TruncateReason(reason);
            }

            if (tokens.Any())
            {
                await _context.SaveChangesAsync(CancellationToken.None, ignoreLogs: true);
            }
        }

        public async Task RevokeTokenFamilyAsync(Guid tokenFamily, string reason)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.TokenFamily == tokenFamily && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = TruncateReason(reason);
            }

            if (tokens.Any())
            {
                await _context.SaveChangesAsync(CancellationToken.None, ignoreLogs: true);
            }
        }

        public async Task<int> CleanupExpiredTokensAsync(int olderThanDays = 60)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

            var expiredTokens = await _context.RefreshTokens
                .Where(rt =>
                    (rt.IsRevoked && rt.RevokedAt < cutoffDate) ||
                    (!rt.IsRevoked && rt.ExpiresAt < cutoffDate))
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(expiredTokens);

            if (expiredTokens.Any())
            {
                await _context.SaveChangesAsync(CancellationToken.None, ignoreLogs: true);
            }

            return expiredTokens.Count;
        }

        private static string GenerateSecureToken()
        {
            Span<byte> tokenBytes = stackalloc byte[TokenByteLength];
            RandomNumberGenerator.Fill(tokenBytes);

            var token = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-", StringComparison.Ordinal)
                .Replace("/", "_", StringComparison.Ordinal)
                .Replace("=", string.Empty, StringComparison.Ordinal);

            return token;
        }

        private static (string token, RefreshToken entity) CreateRefreshToken(
            int customerId,
            byte branchId,
            string? ipAddress,
            string? userAgent,
            Guid? tokenFamily)
        {
            var token = GenerateSecureToken();
            var tokenHash = ComputeSha256Hash(token);
            var family = tokenFamily ?? Guid.NewGuid();

            var refreshToken = new RefreshToken
            {
                TokenHash = tokenHash,
                CustomerId = customerId,
                BranchId = branchId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(TokenExpirationDays),
                IsRevoked = false,
                IpAddress = TrimToLength(ipAddress, 45),
                UserAgent = TrimToLength(userAgent, 500),
                TokenFamily = family
            };

            return (token, refreshToken);
        }

        private static string? TrimToLength(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private static string TruncateReason(string reason)
            => string.IsNullOrEmpty(reason) ? reason : reason.Length > 200 ? reason[..200] : reason;

        private static string ComputeSha256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        private static string BuildRotationCacheKey(string token)
            => string.Concat(RotationCacheKeyPrefix, token);

    }
}
