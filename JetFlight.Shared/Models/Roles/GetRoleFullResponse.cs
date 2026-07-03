namespace JetFlight.Shared.Models.Roles;

public class GetRoleFullResponse : GetRoleResponse
{
    public List<GetRolePermissionsResponse> Permission { get; set; } = default!;

}