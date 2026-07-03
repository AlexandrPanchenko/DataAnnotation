using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JetFlight.Service.Services
{
    /// <summary>
    /// Background service that periodically cleans up expired and revoked refresh tokens
    /// Runs daily at 2 AM UTC
    /// </summary>
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<RefreshTokenCleanupService> _logger;

        public RefreshTokenCleanupService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<RefreshTokenCleanupService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Refresh Token Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Calculate time until next run (2 AM UTC)
                    var now = DateTime.UtcNow;
                    var nextRun = now.Date.AddDays(1).AddHours(2); // Tomorrow at 2 AM UTC

                    // If it's already past 2 AM today and we haven't run yet, run today instead
                    if (now.Hour < 2)
                    {
                        nextRun = now.Date.AddHours(2); // Today at 2 AM UTC
                    }

                    var delay = nextRun - now;

                    _logger.LogInformation(
                        "Next refresh token cleanup scheduled at {NextRun} UTC (in {Hours} hours)",
                        nextRun,
                        delay.TotalHours);

                    // Wait until next scheduled run
                    await Task.Delay(delay, stoppingToken);

                    // Execute cleanup
                    await CleanupExpiredTokensAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Service is being stopped
                    _logger.LogInformation("Refresh Token Cleanup Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Refresh Token Cleanup Service");
                    // Wait 1 hour before retrying on error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task CleanupExpiredTokensAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting refresh token cleanup");

                using var scope = _serviceScopeFactory.CreateScope();
                var refreshTokenService = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

                // Clean up tokens older than 60 days (revoked or expired)
                var deletedCount = await refreshTokenService.CleanupExpiredTokensAsync(olderThanDays: 60);

                _logger.LogInformation(
                    "Refresh token cleanup completed. Deleted {Count} expired/revoked tokens",
                    deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup expired refresh tokens");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Refresh Token Cleanup Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}
