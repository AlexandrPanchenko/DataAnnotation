namespace JetFlight.Shared.Models.ContactUs
{
    public class ContactUsUpdateResponse
    {
        public ContactUsDTO Item { get; set; }
        public List<string> Errors { get; set; }

        public ContactUsUpdateResponse()
        {
            Errors = new List<string>();
        }
    }
}
