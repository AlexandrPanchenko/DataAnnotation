namespace JetFlight.Shared.Models.SendGrid
{
    public class GetContactListResponse
    {
        public List<ExistingContact> Result { get; set; }
    }

    public class ExistingContact : Contact
    {
        public Guid Id { get; set; }
    }
}
