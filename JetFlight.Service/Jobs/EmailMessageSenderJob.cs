using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Extensions;
using JetFlight.Shared.Models.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace JetFlight.Service.Jobs
{
    public class EmailMessageSenderJob : IJob
    {
        public static readonly JobKey Key = new JobKey(nameof(EmailMessageSenderJob), JobConstants.TargetNotificationGroup);

        public int EmailMessageId { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

            var scopeServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var integrationDb = scopeServiceProvider.GetRequiredService<IntegrationDataContext>();
            var notificationService = scopeServiceProvider.GetRequiredService<INotificationService>();
            var targetNotificationService = scopeServiceProvider.GetRequiredService<ITargetNotificationService>();

            var entity = await integrationDb.TargetEmailMessages
                .Include(x => x.Link)
                .Include(x => x.Blocks)
                .ThenInclude(x => x.Link)
                .FirstAsync(x => x.Id == EmailMessageId);

            if (entity.Status == Shared.Models.Message.TargetMessageStatus.Processed)
            {
                return;
            }

            var subject = entity.Theme.GetEnumMemberValue();
            var body = await targetNotificationService.GenerateTargetEmailBodyAsync(entity);

            const int fetchCount = 100;
            List<CustomerEmailMessage> customerMessages;
            
            do
            {
                customerMessages = await integrationDb.CustomerEmailMessages
                    .Include(x => x.Customer)
                    .Where(x => x.EmailMessageId == entity.Id 
                        && x.Status == Shared.Models.Message.ScheduledCustomerMessageStatus.Scheduled
                        && x.Customer.EmailVerified)
                    .Take(fetchCount)
                .ToListAsync();

                foreach (var customerMessage in customerMessages)
                {
                    await notificationService.SendEmailAsync(new EmailMessage
                    {
                        Subject = subject,
                        From = (EmailFrom)entity.BranchId,
                        Body = body,
                        To = new List<string> { customerMessage.Customer.Email }
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