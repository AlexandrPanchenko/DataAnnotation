namespace JetFlight.Shared.Models.Feedback
{
    public class FeedbackAttachmentDTO
    {
        public int Id { get; set; }
        public string MimeType { get; set; }
        public string? Name { get; set; }
        public string FilePath { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
