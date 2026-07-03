using JetFlight.IntegrationDataAccess;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Shared;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.UserContext;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.Service.Services;

public interface IFirebaseService
{
    Task<string?> SendTestMessageAsync(string title, string body, int customerId, string type);
    Task SendMessageAsync(string title, string body, string type, byte branchId, int? customerId);
}

public class FirebaseService : IFirebaseService
{
    private readonly IntegrationDataContext _dataContext;
    private readonly IUserContext _userContext;

    public FirebaseService(IntegrationDataContext dataContext, IUserContext userContext)
    {
        _dataContext = dataContext;
        _userContext = userContext;

        // Initialize FirebaseApp only once per app domain
        if (FirebaseApp.DefaultInstance == null)
        {
            try
            {
                var serviceAccountJson = FirebaseServiceAccount.GetServiceAccountJson();
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromJson(serviceAccountJson)
                });
            }
            catch
            {
                // Swallow initialization errors here; we'll attempt again during send
            }
        }
    }

    public async Task<string?> SendTestMessageAsync(string title, string body, int customerId, string type)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            var serviceAccountJson = FirebaseServiceAccount.GetServiceAccountJson();
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromJson(serviceAccountJson)
            });
        }
        var customerSetting = await _dataContext.CustomerSettings
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.EnablePushNotifications == true);

        if (customerSetting == null || string.IsNullOrEmpty(customerSetting.PushNotificationToken))
            throw new InvalidOperationException($"No valid push notification token found for customer ID: {customerId}");

        var message = new Message
        {
            Token = customerSetting.PushNotificationToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = new Dictionary<string, string>
            {
                { "type", type }
            }
        };

        var messaging = FirebaseMessaging.DefaultInstance;
        var messageId = await messaging.SendAsync(message);

        Console.WriteLine($"Notification sent successfully to customer ID: {customerId}. Message ID: {messageId}");
        return messageId;
    }

    public async Task SendMessageAsync(string title, string body, string type, byte branchId, int? customerId)
    {
        if (customerId == null)
            return;

        var customerSetting = await _dataContext.CustomerSettings
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.BranchId == branchId);

        string? messageId = null;

        if (customerSetting != null && customerSetting.EnablePushNotifications && !string.IsNullOrEmpty(customerSetting.PushNotificationToken))
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var serviceAccountJson = FirebaseServiceAccount.GetServiceAccountJson();
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromJson(serviceAccountJson)
                });
            }
            var message = new Message
            {
                Token = customerSetting.PushNotificationToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = new Dictionary<string, string>
            {
                { "type", type }
            }
            };

            try
            {
                messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered || ex.Message.Contains("Requested entity was not found"))
            {
                customerSetting.PushNotificationToken = null;
                await _dataContext.SaveChangesAsync();
            }
        }

        // Always add to notification history
        var notificationHistory = new NotificationHistory
        {
            CustomerId = customerId.Value,
            BranchId = branchId,
            Title = title ?? string.Empty,
            Body = body ?? string.Empty,
            IsRead = false,
            MessageId = messageId ?? Guid.NewGuid().ToString(),
            Type = type ?? string.Empty,
            CreatedAt = DateTime.UtcNow.SetKindUtc()
        };

        await _dataContext.NotificationHistories.AddAsync(notificationHistory);
        await _dataContext.SaveChangesAsync();
    }
}