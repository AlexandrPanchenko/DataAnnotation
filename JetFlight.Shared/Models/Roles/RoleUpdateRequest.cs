using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Roles;

public class RoleUpdateRequest
{
    [Required]
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Title { get; set; } = default!;
    public List<CreateRolePermissionsResponse> RolePermissions { get; set; } = default!;
}