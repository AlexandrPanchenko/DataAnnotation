using JetFlight.Shared.Models.Message;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class TargetSmsMessage
    {
        public int Id { get; set; }
        public byte BranchId { get; set; }
        public string Message { get; set; }
        public TargetMessageStatus Status { get; set; }
        public MessageTheme Theme { get; set; }
        public int TargetId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ScheduledDate { get; set; }
        public List<CustomerSmsMessage> CustomerSmsMessages { get; set; }
    }

    public class CustomerSmsMessage
    {
        public int Id { get; set; }
        public TargetSmsMessage SmsMessage { get; set; }
        public int SmsMessageId { get; set; }
        public Customer Customer { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ScheduledCustomerMessageStatus Status { get; set; }
    }
}