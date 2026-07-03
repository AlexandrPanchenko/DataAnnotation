using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface ICityRepository : IGenericDataRepository<City>
{
}

public class CityRepository : DataGenericRepository<City>, ICityRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public CityRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }
}
