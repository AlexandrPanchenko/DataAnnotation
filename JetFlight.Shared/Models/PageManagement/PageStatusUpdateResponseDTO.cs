namespace JetFlight.Shared.Models.PageManagement
{
    public class PageStatusUpdateResponseDTO
    {
        public PageDTO Item { get; set; }
        public List<string> Errors { get; set; }

        public PageStatusUpdateResponseDTO()
        {
            Errors = new List<string>();
        }
    }
}