namespace JetFlight.Shared.Models.Feedback
{
    public class ProcessFeedbackRequest
    {
        public int Id { get; set; }
        public string ResolveMessage { get; set; }
        public string ResolveSignature { get; set; }
        public int? CouponIdToAssign { get; set; }
    }
}
