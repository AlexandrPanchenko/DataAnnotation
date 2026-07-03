
namespace JetFlight.ApplicationDataAccess.Repository.DataContext
{
    using JetFlight.ApplicationDataAccess.Entities.DataContext;
    using Microsoft.EntityFrameworkCore;
    using System.Data;

    public interface IRoleRepository : IGenericDataRepository<AdminRole>
    {
    }

    public class RoleRepository : DataGenericRepository<AdminRole>, IRoleRepository
    {
        public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

        public RoleRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
        {
            _dbContext = context;
        }

    }

}
