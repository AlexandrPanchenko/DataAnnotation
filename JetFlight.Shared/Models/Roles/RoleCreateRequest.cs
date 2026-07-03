using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Roles;

public class RoleCreateRequest
{
    [Required]
    public string Name { get; set; } = default!;
    public string Title { get; set; } = default!;
    [Required]
    public List<CreateRolePermissionsResponse> RolePermissions { get; set; } = default!;
}