namespace JetFlight.Shared.Models.Roles;

public class RoleCreateResponse
{
    public GetRoleResponse Item { get; set; }
    public List<string> Errors { get; set; }

    public RoleCreateResponse()
    {
        Errors = new List<string>();
    }
}
