namespace JetFlight.IntegrationDataAccess.Entities
{
    public class CustomerSetting
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? PushNotificationToken { get; set; }
        public byte BranchId { get; set; }
        public bool EnablePushNotifications { get; set; }
        public bool EnableEmailNotifications { get; set; }
        public bool EnableSmsNotifications { get; set; }
        public bool EnableSubscription { get; set; }
        public bool? EnableCookies { get; set; }
        public bool AutomaticWithdrawal { get; set; }
        public bool AccumulateRest { get; set; }
        public int? ActiveStoreId { get; set; }
        public string Avatar { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Customer Customer { get; set; }
    }
}
