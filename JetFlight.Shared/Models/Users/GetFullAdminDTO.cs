using JetFlight.Shared.Models.Roles;

namespace JetFlight.Shared.Models.Admins;

public class GetFullAdminDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public bool SuperAdmin { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool Blocked { get; set; }
    public DateTime? CreatedAt { get; set; }

    public List<GetRoleFullResponse> Roles { get; set; } = default!;
}
