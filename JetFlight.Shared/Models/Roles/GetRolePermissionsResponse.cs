using JetFlight.Shared.UserContext;

namespace JetFlight.Shared.Models.Roles
{
    public class GetRolePermissionsResponse
    {
        public int Id { get; set; }
        public Permission EntityType { get; set; }
        public string Title { get; set; }
        public byte? Crud { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
