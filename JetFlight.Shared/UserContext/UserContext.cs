using JetFlight.Shared.Models.Store;

namespace JetFlight.Shared.UserContext;

public interface IUserContext
{
    public int? AdminId { get; }
    public int? CustomerId { get; }
    public Branches? BranchId { get; }
    public string? ClientId { get; }
}

public class UserContext : IUserContext
{
    public int? AdminId { get; set; }
    public int? CustomerId { get; set; }
    public Branches? BranchId { get; set; }
    public string? ClientId { get; set; }

    public UserContext()
    {
    }
}