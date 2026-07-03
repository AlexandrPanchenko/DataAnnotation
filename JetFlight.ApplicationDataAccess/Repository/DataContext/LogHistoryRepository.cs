using JetFlight.ApplicationDataAccess.Entities.DataContext;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface ILogHistoryRepository : IGenericDataRepository<LogHistory>
{
    IQueryable<LogHistory> GetAll(int adminId, DateTime? timeFrom, DateTime? timeTo);
    IQueryable<LogHistory> GetByEntityId(int entityId, string entityType, DateTime? timeFrom, DateTime? timeTo);

    IQueryable<LogHistory> GetByEntityId(int entityId, IEnumerable<string> entityTypes, DateTime? timeFrom, DateTime? timeTo);
    IQueryable<LogHistory> GetByAdminIdAndEntityType(int adminId, string entityType, DateTime? timeFrom, DateTime? timeTo);
}

public class LogHistoryRepository : DataGenericRepository<LogHistory>, ILogHistoryRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public LogHistoryRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }

    public IQueryable<LogHistory> GetAll(int adminId, DateTime? timeFrom, DateTime? timeTo)
    {
        var query = _context.LogsHistory.Where(x => x.AdminId == adminId).Include(x => x.Admin).AsQueryable();
        return ApplyDateFilter(query, timeFrom, timeTo);
    }

    public IQueryable<LogHistory> GetByEntityId(int entityId, string entityType, DateTime? timeFrom, DateTime? timeTo)
    {
        var query = _context.LogsHistory.Where(x => x.EntityId == entityId && x.EntityType == entityType).Include(x => x.Admin).AsQueryable();
        return ApplyDateFilter(query, timeFrom, timeTo);
    }

    public IQueryable<LogHistory> GetByEntityId(int entityId, IEnumerable<string> entityTypes, DateTime? timeFrom, DateTime? timeTo)
    {
        var query = _context.LogsHistory.Where(x => x.EntityId == entityId && entityTypes.Contains(x.EntityType)).Include(x => x.Admin).AsQueryable();
        return ApplyDateFilter(query, timeFrom, timeTo);
    }

    public IQueryable<LogHistory> GetByAdminIdAndEntityType(int adminId, string entityType, DateTime? timeFrom, DateTime? timeTo)
    {
        var query = this.GetAll(adminId, timeFrom, timeTo).Where(x => x.EntityType == entityType).AsQueryable();
        return query;
    }

    private IQueryable<LogHistory> ApplyDateFilter(IQueryable<LogHistory> query, DateTime? timeFrom, DateTime? timeTo)
    {
        if (timeFrom != null && timeTo != null)
        {
            query = query.Where(x => x.Date >= timeFrom.Value.Date && x.Date <= timeTo.Value.Date.AddDays(1));
        }
        else if (timeFrom == null && timeTo != null)
        {
            query = query.Where(x => x.Date <= timeTo.Value.Date.AddDays(1));
        }
        else if (timeFrom != null && timeTo == null)
        {
            query = query.Where(x => x.Date >= timeFrom.Value.Date);
        }
        return query;
    }
}
