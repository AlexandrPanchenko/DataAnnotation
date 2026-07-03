
namespace JetFlight.Shared.Models.Admins
{
    public class GetAdminsResponseDTO
    {
        public int Total { get; set; }
        public List<GetAdminDTO> Admins { get; set; }
    }
}
