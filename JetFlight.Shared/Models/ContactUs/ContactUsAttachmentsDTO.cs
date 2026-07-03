namespace JetFlight.Shared.Models.ContactUs
{
    public class ContactUsAttachmentsDTO
    {
        public int Id { get; set; }
        public int? ContactUsId { get; set; }
        public string MimeType { get; set; }
        public string? Name { get; set; }    
        public string FilePath { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
