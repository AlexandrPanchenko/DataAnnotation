namespace JetFlight.Shared.Models.Roles;

public class RoleUpdateResponse
{
    public GetRoleResponse Item { get; set; }
    public List<string> Errors { get; set; }

    public RoleUpdateResponse()
    {
        Errors = new List<string>();
    }
}
