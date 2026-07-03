namespace JetFlight.Shared.Models.Feedback
{
    public class FeedbackDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public FeedbackCustomerDTO? Customer { get; set; }
        public byte BranchId { get; set; }
        public int? StoreId { get; set; }
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
        public ICollection<FeedbackAttachmentDTO> Attachments { get; set; } = null;
        public FeedbackType Type => StoreId.HasValue ? FeedbackType.Store : FeedbackType.Branch;
        public List<FeedbackCouponDTO> AssignedCoupons { get; set; }
    }
}
