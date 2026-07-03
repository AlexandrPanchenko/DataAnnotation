namespace JetFlight.Shared.Models.LogHistory;

public class AdminRoleLogHistoryDTO
{
    public int? Id { get; set; } = null;
    public string? Name { get; set; } = null;
    public string? Title { get; set; } = null;
    public bool? isActive { get; set; } = null;

    public List<RoleToPermissionHistoryDto> RoleToPermissions { get; set; } = new List<RoleToPermissionHistoryDto>();
}

public class RoleToPermissionHistoryDto
{
    public int PermissionsId { get; set; }
}
