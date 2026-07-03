using JetFlight.ApplicationDataAccess;
using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Extensions;
using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;
using System.Linq;

namespace JetFlight.Service.Jobs
{
    [PersistJobDataAfterExecution]
    [DisallowConcurrentExecution]
    public class SavedPromotionStatusChangedNotificationJob : IJob
    {
        private const int NotificationWindowStartHour = 8;
        private const int NotificationWindowEndHour = 22;

        public static JobKey Key = new JobKey(nameof(SavedPromotionStatusChangedNotificationJob), JobConstants.LoyaltyGroup);

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.Scheduler.Context.Get("IServiceProvider") as IServiceProvider;

            var scopeServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var integrationDb = scopeServiceProvider.GetRequiredService<IntegrationDataContext>();

            var applicationDb = scopeServiceProvider.GetRequiredService<ApplicationDataContext>();

            var jobSchedulerService = scopeServiceProvider.GetRequiredService<IJobSchedulerService>();

            var notificationService = scopeServiceProvider.GetRequiredService<INotificationService>();

            var firebaseService = scopeServiceProvider.GetRequiredService<IFirebaseService>();

            var smsSettings = scopeServiceProvider.GetRequiredService<IOptions<SmsSettings>>().Value;

            var dataMap = context.JobDetail.JobDataMap;

            var nowUtc = DateTime.UtcNow;
            var nowLocal = nowUtc.FromUtcToTimezone(TimeZoneConstants.UATimezone);

            if (nowLocal.Hour < NotificationWindowStartHour || nowLocal.Hour >= NotificationWindowEndHour)
            {
                return;
            }

            var now = nowUtc;

            if (!dataMap.TryGetDateTime("LastDateTime", out var lastDateTime))
            {
                lastDateTime = await integrationDb.PromotionQueues.OrderBy(x => x.CreatedAt).Select(x => (DateTime?)x.CreatedAt).FirstOrDefaultAsync() ?? now;
            }

            var queueData = await integrationDb.PromotionQueues
                .Select(x => new
                {
                    QueueItem = x,
                    Promotion = integrationDb.Promotions.First(p => p.Id == x.PromotionId),
                })
                .Select(x => new
                {
                    x.QueueItem,
                    x.Promotion,
                    WasActive = x.QueueItem.StartedAt <= lastDateTime && x.QueueItem.ExpiredAt > lastDateTime && x.QueueItem.IsActive,
                    CurrentActive = x.Promotion.StartedAt <= now && x.Promotion.ExpiredAt > now && !x.Promotion.InActive
                })
                .Where(x => x.Promotion.SavePromotions.Any() && !string.IsNullOrEmpty(x.Promotion.ProductCode))
                .GroupBy(x => x.Promotion.ProductCode)
                .Select(x => x.First())
                .ToListAsync();

            var promotionsWithUpdatedStatuses = queueData.Where(x => x.WasActive != x.CurrentActive)
                .Select(x => new
                {
                    x.Promotion,
                    x.CurrentActive
                })
                .ToList();

            var ignoreProductCodes = queueData.Select(x => x.Promotion.ProductCode).ToList();

            var notUpdatedPromotions = await integrationDb.Promotions
                .Where(x => x.SavePromotions.Any() && !string.IsNullOrEmpty(x.ProductCode) && !ignoreProductCodes.Contains(x.ProductCode))
                .Select(x => new
                {
                    WasActive = x.StartedAt <= lastDateTime && x.ExpiredAt > lastDateTime && !x.InActive,
                    CurrentActive = x.StartedAt <= now && x.ExpiredAt > now && !x.InActive,
                    Promotion = x,
                })
                .Where(x => x.WasActive != x.CurrentActive)
                .Select(x => new { x.Promotion, x.CurrentActive })
                .ToListAsync();

            var promotionsToNotify = promotionsWithUpdatedStatuses.Concat(notUpdatedPromotions)
                .Where(x => !string.IsNullOrEmpty(x.Promotion.ProductCode))
                .GroupBy(x => x.Promotion.ProductCode)
                .Select(g => g.First())
                .ToList();

            if (promotionsToNotify.Any())
            {
                var birdjetStoreNumbers = applicationDb.Stores.Where(x => x.BranchId == (byte)Branches.BirdJet).Select(x => x.Number).ToList();
                var catjetStoreNumbers = applicationDb.Stores.Where(x => x.BranchId == (byte)Branches.CatJet).Select(x => x.Number).ToList();

                // Завантажуємо назви товарів для коректного відображення в пушах
                var productCodes = promotionsToNotify
                    .Where(x => x.CurrentActive && !string.IsNullOrEmpty(x.Promotion.ProductCode))
                    .Select(x => x.Promotion.ProductCode!)
                    .Distinct()
                    .ToList();
                var productsByCode = await integrationDb.Products
                    .Where(p => productCodes.Contains(p.Code))
                    .ToDictionaryAsync(p => p.Code, p => p.Title);

                foreach (var promotion in promotionsToNotify)
                {
                    if (!promotion.CurrentActive)
                    {
                        continue; // Do not notify when promotion becomes unavailable
                    }

                    var productTitle = productsByCode.GetValueOrDefault(promotion.Promotion.ProductCode!)
                        ?? promotion.Promotion.Title;
                    var message = $"Акція {productTitle} знову доступна";

                    // Find all branches where this product code has active promotions
                    var productCode = promotion.Promotion.ProductCode;
                    var allPromotionsWithProductCode = await integrationDb.Promotions
                        .Where(p => p.ProductCode == productCode 
                            && p.StartedAt <= now 
                            && p.ExpiredAt > now 
                            && !p.InActive)
                        .Select(p => p.StoreCode)
                        .ToListAsync();

                    var branches = new List<byte>();

                    foreach (var storeCode in allPromotionsWithProductCode)
                    {
                        if (!string.IsNullOrEmpty(storeCode))
                        {
                            if (birdjetStoreNumbers.Contains(storeCode) && !branches.Contains((byte)Branches.BirdJet))
                            {
                                branches.Add((byte)Branches.BirdJet);
                            }

                            if (catjetStoreNumbers.Contains(storeCode) && !branches.Contains((byte)Branches.CatJet))
                            {
                                branches.Add((byte)Branches.CatJet);
                            }
                        }
                    }

                    if (!branches.Any())
                    {
                        continue;
                    }

                    // Find all customers who saved promotions with the same product code in the relevant branches
                    var customerSettings = await integrationDb.CustomerSettings
                        .Include(x => x.Customer)
                        .Where(x => branches.Contains(x.BranchId) 
                            && integrationDb.SavedPromotions.Any(sp => sp.CustomerId == x.CustomerId && sp.Promotion.ProductCode == productCode)
                            && x.Customer != null && !x.Customer.IsDeleted && !x.Customer.IsBlocked)
                        .ToListAsync();

                    var distinctByCustomerAndBranch = customerSettings
                        .GroupBy(x => new { x.CustomerId, x.BranchId })
                        .Select(g => g.OrderByDescending(x => x.EnablePushNotifications).First())
                        .ToList();

                    foreach (var setting in distinctByCustomerAndBranch)
                    {
                        // Лише пуши — без мейлів (за вимогою)
                        await ProcessSmsNotificationsAsync(message, setting, notificationService, smsSettings);
                        await ProcessPushNotificationsAsync(message, setting, firebaseService);
                    }
                }
            }

            await integrationDb.PromotionQueues.Where(x => x.CreatedAt < now).ExecuteDeleteAsync();

            dataMap.Put("LastDateTime", now);
        }

        private static async Task ProcessPushNotificationsAsync(
            string message,
            CustomerSetting setting,
            IFirebaseService firebaseService)
        {

                var title = message;
                var body = message;
                    try
                    {
                        await firebaseService.SendMessageAsync(title, body, "promotions", setting.BranchId, setting.CustomerId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Firebase error for customer ID: {setting.CustomerId}, token: {setting.PushNotificationToken}, error: {ex.Message}");
                    }
            
        }

        private static async Task ProcessSmsNotificationsAsync(
            string message,
            CustomerSetting setting,
            INotificationService notificationService,
            SmsSettings smsSettings)
        {
            if (setting.EnableSmsNotifications && !setting.EnablePushNotifications)
            {
                var smsMessage = new SmsMessage
                {
                    Recievers = new List<string> { setting.Customer.PhoneNumber },
                    Message = message,
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
    }
}
