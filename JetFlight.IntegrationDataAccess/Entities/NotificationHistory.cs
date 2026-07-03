
namespace JetFlight.IntegrationDataAccess.Entities
{
    public class NotificationHistory
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public byte BranchId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool IsRead { get; set; }
        public string MessageId { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
