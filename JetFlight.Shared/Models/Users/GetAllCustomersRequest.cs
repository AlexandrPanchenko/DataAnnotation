
namespace JetFlight.Shared.Models.Users;

public class GetAllCustomersRequest
{
    public string? searchParam { get; set; }
    public string? orderBy { get; set; }
    public string? orderByDirection { get; set; }
    public int? offset { get; set; }
    public int? limit { get; set; }
    public byte? branchId { get; set; }
    public CustomerStatus? customerStatus { get; set; }
    public RegistrationPlatform? registrationPlatform { get; set; }
    public DateTime? dateOfRegistration { get; set; }
    public string? city { get; set; }
}