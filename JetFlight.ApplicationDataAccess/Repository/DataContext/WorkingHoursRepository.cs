
namespace JetFlight.ApplicationDataAccess.Repository.DataContext
{
    using JetFlight.ApplicationDataAccess.Entities.DataContext;
    using Microsoft.EntityFrameworkCore;
    using System.Data;

    public interface IWorkingHoursRepository : IGenericDataRepository<WorkingHours>
    {
    }

    public class WorkingHoursRepository : DataGenericRepository<WorkingHours>, IWorkingHoursRepository
    {
        public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

        public WorkingHoursRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
        {
            _dbContext = context;
        }

    }

}
