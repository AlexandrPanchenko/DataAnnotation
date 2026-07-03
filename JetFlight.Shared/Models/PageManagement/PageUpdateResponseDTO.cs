namespace JetFlight.Shared.Models.PageManagement
{
    public class PageUpdateResponseDTO
    {
        public PageUpdateDTO Item { get; set; }
        public List<string> Errors { get; set; }

        public PageUpdateResponseDTO()
        {
            Errors = new List<string>();
        }
    }
}