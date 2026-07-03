namespace JetFlight.Shared.Models.PageManagement
{
    public class SettingsUpdateResponseDTO
    {
        public SettingsDTO Item { get; set; }
        public List<string> Errors { get; set; }

        public SettingsUpdateResponseDTO()
        {
            Errors = new List<string>();
        }
    }
}