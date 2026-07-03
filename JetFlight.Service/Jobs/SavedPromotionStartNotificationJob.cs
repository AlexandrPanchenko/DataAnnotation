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

namespace JetFlight.Service.Jobs;

public class SavedPromotionStartNotificationJob : IJob
{
    public static JobKey Key = new JobKey(nameof(SavedPromotionStartNotificationJob), JobConstants.LoyaltyGroup);

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

        var dataMap = context.JobDetail.JobDataMap;
        var promotionId = dataMap.GetInt("PromotionId");

        Console.WriteLine($"[SavedPromotionStartNotificationJob] Started for PromotionId={promotionId}");

        if (promotionId == 0)
        {
            Console.WriteLine("[SavedPromotionStartNotificationJob] PromotionId is 0, exiting.");
            return;
        }

        var savedPromotions = await integrationDb.SavedPromotions
            .Include(x => x.Customer)
                .ThenInclude(x => x.CustomerSettings)
            .Include(x => x.Promotion)
                .ThenInclude(x => x.Product)
            .Where(x => x.PromotionId == promotionId
                && x.Customer != null
                && !x.Customer.IsDeleted
                && !x.Customer.IsBlocked)
            .ToListAsync();

        Console.WriteLine(
            $"[SavedPromotionStartNotificationJob] PromotionId={promotionId}, SavedPromotionsCount={savedPromotions.Count}");

        foreach (var savedPromotion in savedPromotions)
        {
            var settings = savedPromotion.Customer.CustomerSettings.ToList();

            foreach (var setting in settings)
            {
                await ProcessEmailNotificationsAsync(savedPromotion, setting, htmlGenerationService, notificationService);

                await ProcessPushNotificationsAsync(savedPromotion, setting, firebaseService);

                if (!setting.EnablePushNotifications)
                {
                    await ProcessSmsNotificationsAsync(savedPromotion, setting, notificationService, smsSettings);
                }
            }
        }
    }

    private static async Task ProcessEmailNotificationsAsync(
        SavedPromotion savedPromotion,
        CustomerSetting setting,
        IHtmlGenerationService htmlGenerationService,
        INotificationService notificationService)
    {
        try
        {
            if (setting.EnableEmailNotifications && savedPromotion.Customer != null && !string.IsNullOrEmpty(savedPromotion.Customer.Email))
            {
                var body = await htmlGenerationService.GenerateSavedPromotionStartsTodayEmail(
                    savedPromotion.Customer.FirstName ?? string.Empty,
                    savedPromotion.Promotion.Title,
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
                    Subject = $"Збережена акція {savedPromotion.Promotion.Title} вже стартувала",
                    Body = body,
                    To = new List<string> { savedPromotion.Customer.Email }
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SavedPromotionStartNotificationJob.ProcessEmailNotificationsAsync] Exception: {ex}");
        }
    }

    private static async Task ProcessPushNotificationsAsync(
        SavedPromotion savedPromotion,
        CustomerSetting setting,
        IFirebaseService firebaseService)
    {
        // Always call SendMessageAsync to ensure notification is added to history
        // SendMessageAsync will send push notification only if EnablePushNotifications is true
        // but will always add the notification to NotificationHistory
        var promotionDisplayTitle = GetPromotionDisplayTitle(savedPromotion.Promotion);
        var title = "Збережена акція розпочалась";
        var body = $"Ваша збережена акція {promotionDisplayTitle} вже доступна. Не проґавте!";

        try
        {
            await firebaseService.SendMessageAsync(title, body, "savedPromotions", setting.BranchId, setting.CustomerId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Firebase error for customer ID: {savedPromotion.Customer.Id}, token: {setting.PushNotificationToken}, error: {ex.Message}");
        }
    }

    private static string GetPromotionDisplayTitle(Promotion promotion)
    {
        // For complex promotions: use Offer if present, otherwise Product.Title or Promotion.Title
        // For regular promotions: use Product.Title or Promotion.Title
        if (promotion.IsComplexPromotion && !string.IsNullOrEmpty(promotion.Offer))
        {
            return promotion.Offer;
        }
        return promotion.Product?.Title ?? promotion.Title;
    }

    private static async Task ProcessSmsNotificationsAsync(
        SavedPromotion savedPromotion,
        CustomerSetting setting,
        INotificationService notificationService,
        SmsSettings smsSettings)
    {
        try
        {
            if (setting.EnableSmsNotifications && savedPromotion.Customer != null && !string.IsNullOrEmpty(savedPromotion.Customer.PhoneNumber))
            {
                var smsMessage = new SmsMessage
                {
                    Recievers = new List<string> { savedPromotion.Customer.PhoneNumber },
                    Message = $"Ваша збережена акція {savedPromotion.Promotion.Title} вже розпочалась. Завітайте до нас!"
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
            Console.WriteLine($"[SavedPromotionStartNotificationJob.ProcessSmsNotificationsAsync] Exception: {ex}");
        }
    }
}

