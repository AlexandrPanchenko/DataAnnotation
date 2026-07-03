using JetFlight.IntegrationDataAccess;
using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace JetFlight.Service.Jobs
{
    public class TargetNotificationReshedulerJob : IJob
    {
        public static readonly JobKey Key = new JobKey(nameof(TargetNotificationReshedulerJob), JobConstants.TargetNotificationGroup);

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

            var scopeServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var jobScheduler = scopeServiceProvider.GetRequiredService<IJobSchedulerService>();

            var integrationDb = scopeServiceProvider.GetRequiredService<IntegrationDataContext>();

            var dateTime = DateTime.UtcNow.AddHours(JobConstants.TargetNotificationJobIntervalHours).AddMinutes(JobConstants.TargetNotificationJobDelayMinutes);

            var sendSmsMessageCandidates = await integrationDb.TargetSmsMessages
                .Where(x => x.Status == Shared.Models.Message.TargetMessageStatus.Scheduled && x.ScheduledDate < dateTime)
                .Select(x => new { x.Id, x.ScheduledDate })
                .ToListAsync();

            foreach (var candidate in sendSmsMessageCandidates)
            {
                await jobScheduler.SetSmsSenderAsync(candidate.Id, candidate.ScheduledDate, true);
            }

            var sendEmailMessageCandidates = await integrationDb.TargetEmailMessages
                .Where(x => x.Status == Shared.Models.Message.TargetMessageStatus.Scheduled && x.ScheduledDate < dateTime)
                .Select(x => new { x.Id, x.ScheduledDate })
                .ToListAsync();

            foreach (var candidate in sendEmailMessageCandidates)
            {
                await jobScheduler.SetEmailSenderAsync(candidate.Id, candidate.ScheduledDate, true);
            }

            sendEmailMessageCandidates.Clear();

            var repopulationSmsCandidates = await integrationDb.TargetSmsMessages
                .Where(x => x.Status == Shared.Models.Message.TargetMessageStatus.Created)
                .Select(x => x.Id)
                .ToListAsync();

            foreach (var candidate in repopulationSmsCandidates)
            {
                await jobScheduler.SetSmsPopulationAsync(candidate, true);
            }

            repopulationSmsCandidates.Clear();

            var repopulationEmailCandidates = await integrationDb.TargetEmailMessages
                .Where(x => x.Status == Shared.Models.Message.TargetMessageStatus.Created)
                .Select(x => x.Id)
                .ToListAsync();

            foreach (var candidate in repopulationEmailCandidates)
            {
                await jobScheduler.SetEmailPopulationAsync(candidate, true);
            }

            repopulationEmailCandidates.Clear();
        }
    }
}