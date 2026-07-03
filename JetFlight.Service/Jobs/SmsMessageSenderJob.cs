using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace JetFlight.Service.Jobs
{
    public class SmsMessageSenderJob : IJob
    {
        public static readonly JobKey Key = new JobKey(nameof(SmsMessageSenderJob), JobConstants.TargetNotificationGroup);

        public int SmsMessageId { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

            var scopeServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var smsSettings = scopeServiceProvider.GetRequiredService<IOptions<SmsSettings>>().Value;

            var integrationDb = scopeServiceProvider.GetRequiredService<IntegrationDataContext>();
            var notificationService = scopeServiceProvider.GetRequiredService<INotificationService>();

            var entity = await integrationDb.TargetSmsMessages
                .FirstAsync(x => x.Id == SmsMessageId);

            if (entity.Status == Shared.Models.Message.TargetMessageStatus.Processed)
            {
                return;
            }

            const int fetchCount = 100;
            List<CustomerSmsMessage> customerMessages;
            
            do
            {
                customerMessages = await integrationDb.CustomerSmsMessages
                    .Include(x => x.Customer)
                    .Where(x => x.SmsMessageId == entity.Id && x.Status == Shared.Models.Message.ScheduledCustomerMessageStatus.Scheduled)
                    .Take(fetchCount)
                    .ToListAsync();

                foreach (var customerMessage in customerMessages)
                {
                    await notificationService.SendSmsAsync(new SmsMessage
                    {
                        Message = entity.Message,
                        Recievers = new List<string> { customerMessage.Customer.PhoneNumber },
                        Sender = entity.BranchId switch
                        {
                            1 => smsSettings.From.BirdJet.Id,
                            2 => smsSettings.From.CatJet.Id
                        },
                    });

                    customerMessage.Status = Shared.Models.Message.ScheduledCustomerMessageStatus.Processed;

                    await integrationDb.SaveChangesAsync();
                }
            }
            while (customerMessages.Count == fetchCount);
            

            entity.Status = Shared.Models.Message.TargetMessageStatus.Processed;

            await integrationDb.SaveChangesAsync();
        }
    }
}