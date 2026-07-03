namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class FeedbackAttachment : ISkipLogHistory
    {
        public int Id { get; set; }
        public int FeedbackId { get; set; }
        public string MimeType { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Feedback Feedback { get; set; }  // Navigation Property
    }
}
