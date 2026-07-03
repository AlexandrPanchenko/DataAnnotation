
namespace JetFlight.ApplicationDataAccess.Repository.DataContext
{
    using JetFlight.ApplicationDataAccess.Entities.DataContext;
    using Microsoft.EntityFrameworkCore;

    public interface ISectionFieldRepository : IGenericDataRepository<Entities.DataContext.SectionField>
    {
    }

    public class SectionFieldRepository : DataGenericRepository<Entities.DataContext.SectionField>, ISectionFieldRepository
    {
        public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

        public SectionFieldRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
        {
            _dbContext = context;
        }

    }

}
