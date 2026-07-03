using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Services;
using JetFlight.Shared;
using JetFlight.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace JetFlight.Service.Jobs
{
    public class SmsPopulationJob : IJob
    {
        public static readonly JobKey Key = new JobKey(nameof(SmsPopulationJob), JobConstants.TargetNotificationGroup);

        public int SmsMessageId { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

            var scopeServiceProvider = serviceProvider.CreateScope().ServiceProvider;
            var scopeServiceForDbConnection = serviceProvider.CreateScope().ServiceProvider;

            var integrationDb = scopeServiceProvider.GetRequiredService<IntegrationDataContext>();
            var integrationDbForManualSqlConnection = scopeServiceForDbConnection.GetRequiredService<IntegrationDataContext>();

            var jobScheduler = scopeServiceProvider.GetRequiredService<IJobSchedulerService>();

            var smsEntity = await integrationDb.TargetSmsMessages.FirstAsync(x => x.Id == SmsMessageId);

            if (smsEntity.Status != Shared.Models.Message.TargetMessageStatus.Created)
            {
                return;
            }

            var targetService = scopeServiceProvider.GetRequiredService<ITargetService>();

            var target = await targetService.GetTargetEntityAsync(smsEntity.TargetId);

            var connection = integrationDbForManualSqlConnection.Database.GetDbConnection();
            await connection.OpenAsync();

            await targetService.PopulateTargetCustomersIntoTempTableAsync(target, connection);

            const int perBatch = 100;
            int skip = 0;
            int fetchedCount;

            do
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id
                    FROM #customers
                    WHERE BranchId = @BranchId
                    ORDER BY Id
                    OFFSET @Skip ROWS FETCH NEXT @PerBatch ROWS ONLY;
                ";

                var perBatchParam = command.CreateParameter();
                perBatchParam.ParameterName = "@PerBatch";
                perBatchParam.Value = perBatch;
                command.Parameters.Add(perBatchParam);

                var skipParam = command.CreateParameter();
                skipParam.ParameterName = "@Skip";
                skipParam.Value = skip;
                command.Parameters.Add(skipParam);

                var branchIdParam = command.CreateParameter();
                branchIdParam.ParameterName = "@BranchId";
                branchIdParam.Value = smsEntity.BranchId;
                command.Parameters.Add(branchIdParam);

                using var reader = await command.ExecuteReaderAsync();

                fetchedCount = 0;

                while (await reader.ReadAsync())
                {
                    var customerId = reader.GetInt32(0);

                    if (!await integrationDb.CustomerSmsMessages.AnyAsync(x => x.Id == SmsMessageId && x.CustomerId == customerId)
                        && await integrationDb.CustomerSettings.AnyAsync(x => x.CustomerId == customerId && x.BranchId == smsEntity.BranchId && x.EnableSmsNotifications == true))
                    {
                        var messageEntity = new CustomerSmsMessage
                        {
                            CustomerId = customerId,
                            SmsMessageId = SmsMessageId,
                            CreatedAt = DateTime.UtcNow.SetKindUtc(),
                            Status = Shared.Models.Message.ScheduledCustomerMessageStatus.Scheduled,
                        };

                        await integrationDb.AddAsync(messageEntity);

                        await integrationDb.SaveChangesAsync();
                    }

                    fetchedCount++;
                }

                skip += perBatch;
            }
            while (fetchedCount == perBatch);

            smsEntity.Status = Shared.Models.Message.TargetMessageStatus.Scheduled;
            await integrationDb.SaveChangesAsync();

            var dateTime = DateTime.UtcNow.AddHours(JobConstants.TargetNotificationJobIntervalHours).AddMinutes(JobConstants.TargetNotificationJobDelayMinutes);

            if (smsEntity.ScheduledDate < dateTime)
            {
                await jobScheduler.SetSmsSenderAsync(smsEntity.Id, smsEntity.ScheduledDate, true);
            }
        }
    }
}