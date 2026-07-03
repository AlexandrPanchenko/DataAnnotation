namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class ContactUsAttachment
    {
        public int Id { get; set; }
        public int? ContactUsId { get; set; }
        public string MimeType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime? CreatedAt { get; set; }

        public ContactUs ContactUs { get; set; }
    }
}
