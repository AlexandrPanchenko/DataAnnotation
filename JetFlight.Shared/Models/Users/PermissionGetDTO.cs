using JetFlight.Shared.Models.Roles;

namespace JetFlight.Shared.Models.Admins
{
    public class PermissionGetDTO
    {
        public int PermissionId { get; set; }
        public AdminUpdateDTO Admin { get; set; }
        public RoleDTO Role { get; set; }
    }
}
