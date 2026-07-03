using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using JetFlight.ApplicationDataAccess.DbFunctions;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface IStoreRepository : IGenericDataRepository<Store>
{
    IQueryable<Store> GetAllStores();
    Task<List<Store>> GetClosestStores(byte branchId, string latitude, string longitude, int? limit);
}

public class StoreRepository : DataGenericRepository<Store>, IStoreRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public StoreRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }

    public IQueryable<Store> GetAllStores()
    {
        return _dbContext.Stores.Include(x => x.City).Include(x => x.MediaFile).Include(x => x.WorkingHours).AsQueryable();
    }

    public Task<List<Store>> GetClosestStores(byte branchId, string latitude, string longitude, int? limit)
    {
        var query = GetAllStores()
            .Where(x => x.BranchId == branchId && !string.IsNullOrWhiteSpace(x.Latitude) && !string.IsNullOrWhiteSpace(x.Longitude) && x.isActive)
            .OrderBy(x => CustomFunctions.CalculateDistance(x.Latitude, x.Longitude, latitude, longitude))
            .AsQueryable();

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return query.ToListAsync();
    }
}
