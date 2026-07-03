namespace JetFlight.Shared.Models.Mouseflow
{
    public class MouseflowRecordingItem
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Referrer { get; set; }
        public string ReferrerType { get; set; }
        public string Entry { get; set; }
        public string EntryPage { get; set; }
    }
}
