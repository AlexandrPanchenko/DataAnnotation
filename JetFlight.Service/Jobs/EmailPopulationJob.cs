using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Services;
using JetFlight.Shared;
using JetFlight.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Xml;

namespace JetFlight.Service.Jobs
{
    public class EmailPopulationJob : IJob
    {
        public static readonly JobKey Key = new JobKey(nameof(EmailPopulationJob), JobConstants.TargetNotificationGroup);

        public int EmailMessageId { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

            var scopeServiceProvider = serviceProvider.CreateScope().ServiceProvider;
            var scopeServiceForDbConnection = serviceProvider.CreateScope().ServiceProvider;

            var integrationDb = scopeServiceProvider.GetRequiredService<IntegrationDataContext>();
            var integrationDbForManualSqlConnection = scopeServiceForDbConnection.GetRequiredService<IntegrationDataContext>();

            var jobScheduler = scopeServiceProvider.GetRequiredService<IJobSchedulerService>();

            var emailEntity = await integrationDb.TargetEmailMessages.FirstAsync(x => x.Id == EmailMessageId);

            if (emailEntity.Status != Shared.Models.Message.TargetMessageStatus.Created)
            {
                return;
            }

            var targetService = scopeServiceProvider.GetRequiredService<ITargetService>();

            var target = await targetService.GetTargetEntityAsync(emailEntity.TargetId);

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
                branchIdParam.Value = emailEntity.BranchId;
                command.Parameters.Add(branchIdParam);

                using var reader = await command.ExecuteReaderAsync();

                fetchedCount = 0;

                while (await reader.ReadAsync())
                {
                    var customerId = reader.GetInt32(0);

                    var customer = await integrationDb.Customers
                        .Include(x => x.CustomerSettings)
                        .FirstOrDefaultAsync(x => x.Id == customerId);
                    
                    if (customer != null 
                        && !string.IsNullOrEmpty(customer.Email)
                        && customer.EmailVerified
                        && !await integrationDb.CustomerEmailMessages.AnyAsync(x => x.Id == EmailMessageId && x.CustomerId == customerId)
                        && customer.CustomerSettings.Any(x => x.BranchId == emailEntity.BranchId && x.EnableEmailNotifications == true))
                    {
                        var messageEntity = new CustomerEmailMessage
                        {
                            CustomerId = customerId,
                            EmailMessageId = EmailMessageId,
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

            emailEntity.Status = Shared.Models.Message.TargetMessageStatus.Scheduled;
            await integrationDb.SaveChangesAsync();

            var dateTime = DateTime.UtcNow.AddHours(JobConstants.TargetNotificationJobIntervalHours).AddMinutes(JobConstants.TargetNotificationJobDelayMinutes);

            if (emailEntity.ScheduledDate < dateTime)
            {
                await jobScheduler.SetEmailSenderAsync(emailEntity.Id, emailEntity.ScheduledDate, true);
            }
        }
    }
}