using JetFlight.ApplicationDataAccess.Entities.DataContext;
using Microsoft.EntityFrameworkCore;
using JetFlight.Shared.Constants;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface ISiteSettingsRepository : IGenericDataRepository<SiteSettings>
{
    Task<Dictionary<SiteSettingsKeys, string>> GetAllSettings(byte branchId);
}

public class SiteSettingsRepository : DataGenericRepository<SiteSettings>, ISiteSettingsRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public SiteSettingsRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }

    public async Task<Dictionary<SiteSettingsKeys, string>> GetAllSettings(byte branchId)
    {
        return await _dbContext.SiteSettings
                       .Where(x => x.BranchId == branchId)
                       .AsNoTracking()
                       .ToDictionaryAsync(p => p.Key, p => p.Value);
    }
}
