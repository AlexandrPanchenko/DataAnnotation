using JetFlight.Shared.Models.Store;

namespace JetFlight.IntegrationDataAccess.Entities
{
    /// <summary>
    /// Refresh token entity for customer authentication with token rotation support.
    /// Stores hashed tokens for security (never store plaintext tokens).
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// SHA256 hash of the refresh token (never store plaintext)
        /// </summary>
        public string TokenHash { get; set; }

        /// <summary>
        /// Customer ID - foreign key to Customer
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Branch ID for branch-specific tokens
        /// </summary>
        public byte BranchId { get; set; }

        /// <summary>
        /// Token creation timestamp (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Token expiration timestamp (UTC) - 30 days from creation
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Revocation flag - soft delete for audit trail
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// Timestamp when token was revoked (UTC)
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Reason for revocation (e.g., "User logout", "Phone changed", "Customer blocked")
        /// </summary>
        public string? RevokedReason { get; set; }

        /// <summary>
        /// IP address from which the token was created
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent of the client that created the token
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Token family ID for token rotation chain tracking.
        /// All tokens in a rotation chain share the same family ID.
        /// Used to detect token reuse attacks.
        /// </summary>
        public Guid TokenFamily { get; set; }

        /// <summary>
        /// Hash of the token that was replaced during rotation (nullable for initial tokens)
        /// </summary>
        public string? ReplacedByTokenHash { get; set; }

        /// <summary>
        /// Timestamp when this token was replaced during rotation (for grace period handling)
        /// </summary>
        public DateTime? ReplacedAt { get; set; }

        /// <summary>
        /// Navigation property to Customer
        /// </summary>
        public Customer Customer { get; set; }
    }
}