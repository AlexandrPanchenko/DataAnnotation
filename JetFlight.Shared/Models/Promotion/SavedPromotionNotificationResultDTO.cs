namespace JetFlight.Shared.Models.Promotion;

public class SavedPromotionNotificationResultDTO
{
    public DateTime ExecutionStartedAt { get; set; }
    public DateTime ExecutionCompletedAt { get; set; }
    public string DateRangeUsed { get; set; } = string.Empty;
    public int PromotionsProcessed { get; set; }
    public int TotalSavedPromotionsFound { get; set; }
    public int TotalCustomersProcessed { get; set; }
    public int TotalPushNotificationsSent { get; set; }
    public int TotalPushNotificationsAddedToHistory { get; set; }
    public int TotalEmailNotificationsSent { get; set; }
    public int TotalSmsNotificationsSent { get; set; }
    public List<PromotionNotificationResultDTO> PromotionResults { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class PromotionNotificationResultDTO
{
    public int PromotionId { get; set; }
    public string? PromotionTitle { get; set; }
    public DateTime? PromotionExpiredAt { get; set; }
    public int SavedPromotionsFound { get; set; }
    public int CustomersProcessed { get; set; }
    public int PushNotificationsSent { get; set; }
    public int PushNotificationsAddedToHistory { get; set; }
    public int EmailNotificationsSent { get; set; }
    public int SmsNotificationsSent { get; set; }
    public List<CustomerNotificationDetailDTO> CustomerDetails { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class CustomerNotificationDetailDTO
{
    public int SavedPromotionId { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhoneNumber { get; set; }
    public byte BranchId { get; set; }
    public bool ExpiresToday { get; set; }
    public bool PushNotificationsEnabled { get; set; }
    public bool HasPushToken { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public bool SmsNotificationsEnabled { get; set; }
    public NotificationStatus PushNotificationStatus { get; set; }
    public NotificationStatus EmailNotificationStatus { get; set; }
    public NotificationStatus SmsNotificationStatus { get; set; }
    public string? PushNotificationMessageId { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum NotificationStatus
{
    NotProcessed,
    Skipped,
    Sent,
    AddedToHistory,
    Failed
}
