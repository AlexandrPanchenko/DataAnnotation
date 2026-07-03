
namespace JetFlight.ApplicationDataAccess.Repository.DataContext
{
    using JetFlight.ApplicationDataAccess.Entities.DataContext;
    using Microsoft.EntityFrameworkCore;
    using System.Data;

    public interface IRolePolicyRepository : IGenericDataRepository<RoleToPermission>
    {
    }

    public class RolePolicyRepository : DataGenericRepository<RoleToPermission>, IRolePolicyRepository
    {
        public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

        public RolePolicyRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
        {
            _dbContext = context;
        }

    }

}
