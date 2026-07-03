namespace JetFlight.Shared.Models.Users
{
    public class AddCustomerCardDTOResponse
    {
        public CustomerCardDTO Item { get; set; }
        public List<string> Errors { get; set; }

        public AddCustomerCardDTOResponse()
        {
            Errors = new List<string>();
        }
    }
}
