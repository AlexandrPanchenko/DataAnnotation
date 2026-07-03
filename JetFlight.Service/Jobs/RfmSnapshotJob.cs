using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace JetFlight.Service.Jobs
{
    public class RfmSnapshotJob : IJob
    {
        public static readonly JobKey Key = new JobKey(nameof(RfmSnapshotJob), JobConstants.AnalyticsGroup);

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = (IServiceProvider)context.Scheduler.Context.Get("IServiceProvider");

            using var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<RfmSnapshotJob>>();
            var snapshotService = scope.ServiceProvider.GetRequiredService<IRfmSnapshotService>();

            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneConstants.UATimezone);
                var nowInZone = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone);
                var snapshotDate = DateOnly.FromDateTime(nowInZone.AddDays(-1));

                logger.LogInformation("Executing RFM snapshot job for {SnapshotDate}", snapshotDate);

                await snapshotService.GenerateDailySnapshotAsync(snapshotDate, context.CancellationToken);

                logger.LogInformation("Completed RFM snapshot job for {SnapshotDate}", snapshotDate);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate RFM customer snapshots.");
                throw;
            }
        }
    }
}
