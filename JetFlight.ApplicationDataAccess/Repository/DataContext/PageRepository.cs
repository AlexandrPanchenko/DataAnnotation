using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface IPageRepository : IGenericDataRepository<Page>
{
    IQueryable<Page> GetRootPages();
    IQueryable<Page> GetAllSubPages(int pageID);
    IQueryable<Page> GetAllPages();
}

public class PageRepository : DataGenericRepository<Page>, IPageRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public PageRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }

    public IQueryable<Page> GetAllPages()
    {
        return _dbContext.Page.Include(x => x.Sections).ThenInclude(x => x.SectionFields).Include(x => x.Origin).AsQueryable();
    }
    public IQueryable<Page> GetRootPages()
    {
        return _dbContext.Page.Include(x => x.Sections).ThenInclude(x => x.SectionFields).Include(x => x.Origin).Where(x => x.OriginId == null && x.ParentId == null && x.Link != null).AsQueryable();
    }

    public IQueryable<Page> GetAllSubPages(int pageID)
    {
        return _dbContext.Page.Include(x => x.Sections).ThenInclude(x => x.SectionFields).Include(x => x.Origin).Where(x => x.ParentId == pageID).AsQueryable();
    }
}
