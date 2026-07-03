using JetFlight.ApplicationDataAccess;
using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quartz;

namespace JetFlight.Service.Jobs
{
    public class CouponActiveNotificationJob : IJob
    {
        public static JobKey Key = new JobKey(nameof(CouponActiveNotificationJob), JobConstants.LoyaltyGroup);
        public int CouponId { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

            var scopeServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var integrationDb = scopeServiceProvider.GetRequiredService<IntegrationDataContext>();
            var appDb = scopeServiceProvider.GetRequiredService<ApplicationDataContext>();

            var htmlGenerationService = scopeServiceProvider.GetRequiredService<IHtmlGenerationService>();
            var smsSettings = scopeServiceProvider.GetRequiredService<IOptions<SmsSettings>>().Value;

            var notificationService = scopeServiceProvider.GetRequiredService<INotificationService>();
            var firebaseService = scopeServiceProvider.GetRequiredService<IFirebaseService>();

            var coupon = await integrationDb.Coupons
                .Include(x => x.StoreCodes)
                .FirstAsync(x => x.Id == CouponId);

            if (coupon.Status != Shared.Models.Coupons.CouponStatus.Active)
            {
                return;
            }

            var storeIds = coupon.StoreCodes.Select(x => x.StoreCode).ToList();

            var entitities = await integrationDb.CustomerCoupons
                .Include(x => x.Customer)
                    .ThenInclude(x => x.CustomerSettings)
                .Where(x => x.CouponId == CouponId && x.Coupon.Status == Shared.Models.Coupons.CouponStatus.Active
                 && x.Customer != null && !x.Customer.IsDeleted && !x.Customer.IsBlocked)
                .ToListAsync();

            var branchIds = await appDb.Stores.Where(x => storeIds.Contains(x.Number)).Select(x => x.BranchId).Distinct().ToListAsync();

            // Якщо ваучер призначено для однієї гілки — надсилати тільки для неї
            foreach (var entity in entitities)
            {
                // Знаходимо тільки ту CustomerSetting, яка відповідає гілці ваучера
                var setting = entity.Customer.CustomerSettings.FirstOrDefault(x => branchIds.Contains(x.BranchId));
                if (setting == null)
                    continue;

                await ProcessEmailNotificationsAsync(entity, setting, htmlGenerationService, notificationService);

                if (setting.EnablePushNotifications)
                {
                    await ProcessPushNotificationsAsync(entity, setting, firebaseService);
                }
                else
                {
                    await ProcessSmsNotificationsAsync(entity, setting, notificationService, smsSettings);
                }
            }
        }

        private static async Task ProcessEmailNotificationsAsync(
            CustomerCoupon entity,
            CustomerSetting setting,
            IHtmlGenerationService htmlGenerationService,
            INotificationService notificationService)
        {
            try
            {
                if (setting.EnableEmailNotifications && entity.Customer != null && !string.IsNullOrEmpty(entity.Customer.Email))
                {
                    var body = await htmlGenerationService.GenerateCouponIsAvailableEmail(
                        entity.Customer.FirstName ?? string.Empty,
                        entity.Coupon.Name,
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
                        Subject = $"Ваучер {entity.Coupon.Name} доступний для використання",
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
            CustomerCoupon entity,
            CustomerSetting setting,
            IFirebaseService firebaseService)
        {

                var title = "Ваучер доступний";
                var body = $"Ваш ваучер {entity.Coupon.Name} вже доступний до використання";


                    try
                    {
                        await firebaseService.SendMessageAsync(title, body, "coupons", setting.BranchId, setting.CustomerId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Firebase error for customer ID: {entity.Customer.Id}, token: {setting.PushNotificationToken}, error: {ex.Message}");
                    }
        }

        private static async Task ProcessSmsNotificationsAsync(
            CustomerCoupon entity,
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
                        Message = $"Ваш ваучер {entity.Coupon.Name} вже доступний"
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
