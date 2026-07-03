namespace JetFlight.Shared.Models.PageManagement
{
    public class SectionUpdateResponseDTO
    {
        public SectionDTO Item { get; set; }
        public List<string> Errors { get; set; }

        public SectionUpdateResponseDTO()
        {
            Errors = new List<string>();
        }
    }
}