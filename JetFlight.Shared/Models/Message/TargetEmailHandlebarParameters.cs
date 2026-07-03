namespace JetFlight.Shared.Models.Message
{
    public class TargetEmailHandlebarParameters
    {
        public string? mainImageUrl { get; set; }
        public string? mainHeader { get; set; }
        public string? secondHeader { get; set; }
        public string? text { get; set; }
        public EmailLinkDTO? link { get; set; }
        public List<EmailBlockDTO> blocks { get; set; } = new List<EmailBlockDTO>();
        public string branchEmail { get; set; }
        public string logoUrl { get; set; }
    }
}
