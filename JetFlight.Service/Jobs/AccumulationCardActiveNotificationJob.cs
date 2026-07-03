using JetFlight.ApplicationDataAccess;
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
    public class AccumulationCardActiveNotificationJob : IJob
    {
        public static JobKey Key = new JobKey(nameof(AccumulationCardActiveNotificationJob), JobConstants.LoyaltyGroup);

        public int AccumulationCardId { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

            var scopeServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var integrationDb = scopeServiceProvider.GetRequiredService<IntegrationDataContext>();
            var appDb = scopeServiceProvider.GetRequiredService<ApplicationDataContext>();

            var htmlGenerationService = scopeServiceProvider.GetRequiredService<IHtmlGenerationService>();
            var smsSettings = scopeServiceProvider.GetRequiredService<IOptions<SmsSettings>>().Value;
            var firebaseService = scopeServiceProvider.GetRequiredService<IFirebaseService>();
            var notificationService = scopeServiceProvider.GetRequiredService<INotificationService>();

            var card = await integrationDb.AccumulationCards
                .Include(x => x.Coupons)
                    .ThenInclude(x => x.StoreCodes)
                .Where(x => x.Id == AccumulationCardId)
                .FirstAsync();

            if (card.Status != Shared.Models.AccumulationCard.AccumulationCardStatus.Active)
            {
                return;
            }

            var entitities = await integrationDb.CustomerAccumulationCards
                .Include(x => x.Customer)
                    .ThenInclude(x => x.CustomerSettings)
                .Where(x => x.AccumulationCardId == AccumulationCardId 
                && x.AccumulationCard.Status == Shared.Models.AccumulationCard.AccumulationCardStatus.Active
                    && x.Customer != null && !x.Customer.IsDeleted && !x.Customer.IsBlocked )
                .ToListAsync();

            var firstCoupon = card.Coupons.First();

            var storeCodes = firstCoupon
                .StoreCodes.Select(x => x.StoreCode).ToList();

            var branchIds = await appDb.Stores.Where(x => storeCodes.Contains(x.Number)).Select(x => x.BranchId).Distinct().ToListAsync();

            foreach (var entity in entitities)
            {
                var settings = entity.Customer.CustomerSettings.Where(x => branchIds.Contains(x.BranchId)).ToList();

                foreach (var setting in settings)
                {
                    await ProcessEmailNotificationsAsync(entity, setting, htmlGenerationService, notificationService);

                    await ProcessPushNotificationsAsync(entity, setting, firebaseService);

                    if(!setting.EnablePushNotifications)
                    {
                        await ProcessSmsNotificationsAsync(entity, setting, notificationService, smsSettings);
                    }
                }
            }
        }

        private static async Task ProcessEmailNotificationsAsync(
            CustomerAccumulationCard entity,
            CustomerSetting setting,
            IHtmlGenerationService htmlGenerationService,
            INotificationService notificationService)
        {
            try
            {
                if (setting.EnableEmailNotifications && entity.Customer != null && !string.IsNullOrEmpty(entity.Customer.Email))
                {
                    var body = await htmlGenerationService.GenerateAccumulationCardIsAvailableEmail(
                        entity.Customer.FirstName ?? string.Empty,
                        entity.AccumulationCard.Name,
                        setting.BranchId);

                    EmailFrom emailFrom;
                    if (!Enum.IsDefined(typeof(EmailFrom), (int)setting.BranchId))
                    {
                        throw new ArgumentException($"Invalid BranchId for EmailFrom: {setting.BranchId}");
                    }
                    emailFrom = (EmailFrom)(int)setting.BranchId;

                    await notificationService.SendEmailAsync(new EmailMessage
                    {
                        From = emailFrom,
                        Subject = $"Картка +1 {entity.AccumulationCard.Name} доступна для використання",
                        Body = body,
                        To = new List<string> { entity.Customer.Email }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProcessEmailNotificationsAsync] Exception: {ex}");
            }
        }

        private static async Task ProcessPushNotificationsAsync(
            CustomerAccumulationCard entity,
            CustomerSetting setting,
            IFirebaseService firebaseService)
        {
            if (setting.EnablePushNotifications)
            {
                var title = "Карта +1 доступна";
                var body = $"Ваша картка +1 {entity.AccumulationCard.Name} вже доступна до використання";

                    try
                    {
                        await firebaseService.SendMessageAsync(title, body, "accumulationCards", setting.BranchId, setting.CustomerId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Firebase error for customer ID: {entity.Customer.Id}, token: {setting.PushNotificationToken}, error: {ex.Message}");
                    }
            }
        }

        private static async Task ProcessSmsNotificationsAsync(
            CustomerAccumulationCard entity,
            CustomerSetting setting,
            INotificationService notificationService,
            SmsSettings smsSettings)
        {
            try
            {
                if (setting.EnableSmsNotifications && entity.Customer != null && !string.IsNullOrEmpty(entity.Customer.PhoneNumber))
                {
                    var smsMessage = new SmsMessage
                    {
                        Recievers = new List<string> { entity.Customer.PhoneNumber },
                        Message = $"Ваша картка +1 {entity.AccumulationCard.Name} вже доступна"
                    };

                    switch (setting.BranchId)
                    {
                        case 1:
                            smsMessage.Sender = smsSettings.From.BirdJet.Id;
                            break;
                        case 2:
                            smsMessage.Sender = smsSettings.From.CatJet.Id;
                            break;
                        default:
                            throw new ArgumentException("Invalid branch ID");
                    }

                    await notificationService.SendSmsAsync(smsMessage);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProcessSmsNotificationsAsync] Exception: {ex}");
            }
        }
    }
}
