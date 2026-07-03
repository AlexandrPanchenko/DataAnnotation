using JetFlight.Shared.Models.Admins;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext
{
    public class AdminCreateResponseDTO
    {
        public GetAdminDTO Item { get; set; }
        public List<string> Errors { get; set; }

        public AdminCreateResponseDTO()
        {
            Errors = new List<string>();
        }
    }
}
