using JetFlight.Shared.Models.Message;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class TargetEmailMessage
    {
        public int Id { get; set; }

        public byte BranchId { get; set; }

        public MessageTheme Theme { get; set; }

        public string? MainImageUrl { get; set; }
        public string? MainHeader { get; set; }
        public string? SecondHeader { get; set; }
        public string? Text { get; set; }
        public int? LinkId { get; set; }
        public EmailLink? Link { get; set; }
        public List<EmailBlock> Blocks { get; set; } = new List<EmailBlock>();

        public TargetMessageStatus Status { get; set; }
        public int TargetId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ScheduledDate { get; set; }
    }

    public class EmailLink
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public required string Text { get; set; }
    }

    public class EmailBlock
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public string? Header { get; set; }
        public string? Text { get; set; }
        public int? LinkId { get; set; }
        public EmailLink? Link { get; set; }
    }

    public class CustomerEmailMessage
    {
        public int Id { get; set; }
        public TargetEmailMessage EmailMessage { get; set; }
        public int EmailMessageId { get; set; }
        public Customer Customer { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ScheduledCustomerMessageStatus Status { get; set; }
    }

}
