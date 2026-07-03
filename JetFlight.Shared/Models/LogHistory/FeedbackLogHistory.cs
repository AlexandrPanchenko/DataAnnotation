using JetFlight.Shared.Models.Feedback;

namespace JetFlight.Shared.Models.LogHistory
{
    public class FeedbackLogHistoryDTO
    {
        public DateTime? ProcessingDate { get; set; }
        public string ResolveMessage { get; set; }
        public string ResolveSignature { get; set; }
        public FeedbackStatus Status { get; set; }
    }
}
