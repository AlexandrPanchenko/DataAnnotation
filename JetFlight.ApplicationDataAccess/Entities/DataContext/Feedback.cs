using JetFlight.Shared.Models;
using JetFlight.Shared.Models.Feedback;

namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class Feedback
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public byte BranchId { get; set; }
        public int? StoreId { get; set; }
        public Store Store { get; set; }
        public ClientPlatform Platform { get; set; }
        public byte Rating { get; set; }
        public string Message { get; set; }
        public string ResolveMessage { get; set; }
        public string ResolveSignature { get; set; }
        public int? AssigneeId { get; set; }
        public FeedbackStatus Status { get; set; }
        public DateTime? ProcessingDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Admin Assignee { get; set; }
        public ICollection<FeedbackAttachment> Attachments { get; set; } = null;
        public List<int> CustomerCouponIds { get; set; }
    }
}
