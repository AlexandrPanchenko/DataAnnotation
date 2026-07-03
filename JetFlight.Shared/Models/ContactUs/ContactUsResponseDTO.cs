namespace JetFlight.Shared.Models.ContactUs
{
    public class ContactUsResponseDTO
    {
        public int total { get; set; }
        public List<GetContactUsResponseDTO> contactUsDtos { get; set; } = default!;
    }
}
