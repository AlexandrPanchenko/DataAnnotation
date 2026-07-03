using JetFlight.Shared.Models.Users;
using JetFlight.Shared.UserContext;

namespace JetFlight.Shared.Models.Admins
{
    public class AdminFilterDTO
    {
       public Permission pagePermissionType { get; set; }
       public List<PermissionLevel>? crud { get; set; } = null;
       public string? searchParam { get; set; } = null;
    }
}
