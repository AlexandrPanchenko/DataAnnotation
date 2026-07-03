using JetFlight.Shared.Models.Targets;

namespace JetFlight.Shared.Models.Message
{
    public class CreateTargetSmsMessageDTO
    {
        public byte branchId { get; set; }
        public string message { get; set; }
        public DateTime scheduledDate { get; set; }
        public MessageTheme theme { get; set; }
        public int targetId { get; set; }
    }

    public class TargetSmsMessageDTO
    {
        public int Id { get; set; }
        public byte BranchId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TargetMessageStatus Status { get; set; }
        public MessageTheme Theme { get; set; }
        public int TargetId { get; set; }
        public TargetDto Target { get; set; }
        public int PopulatedMessagesCount { get; set; }
    }

    public class ScheduledCustomerSmsMessageDTO
    {
        public int Id { get; set; }
        public int SmsMessageId { get; set; }
        public byte BranchId { get; set; }
        public string Message { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string CustomerAvatar { get; set; }
        public MessageTheme Theme { get; set; }
        public ScheduledCustomerMessageStatus Status { get; set; }
    }
}