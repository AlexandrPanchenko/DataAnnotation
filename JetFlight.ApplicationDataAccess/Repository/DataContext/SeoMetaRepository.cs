using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface ISeoMetaRepository : IGenericDataRepository<SeoMeta>
{
}

public class SeoMetaRepository : DataGenericRepository<SeoMeta>, ISeoMetaRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public SeoMetaRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }
}
