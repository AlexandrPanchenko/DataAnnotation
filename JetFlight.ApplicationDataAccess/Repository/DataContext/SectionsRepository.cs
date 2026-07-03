
namespace JetFlight.ApplicationDataAccess.Repository.DataContext
{
    using JetFlight.ApplicationDataAccess.Entities.DataContext;
    using Microsoft.EntityFrameworkCore;

    public interface ISectionsRepository : IGenericDataRepository<Entities.DataContext.Section>
    {
        IQueryable<Entities.DataContext.Section> GetWithSectionField(int sectionId);
        IQueryable<Entities.DataContext.Section> GetWithPage(int sectionId);
        IQueryable<Entities.DataContext.Section> GetAllWithPage();
    }

    public class SectionsRepository : DataGenericRepository<Entities.DataContext.Section>, ISectionsRepository
    {
        public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

        public SectionsRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
        {
            _dbContext = context;
        }

        public IQueryable<Entities.DataContext.Section> GetWithSectionField(int sectionId)
        {
            return _context.Sections.Include(x => x.SectionFields).Where(x => x.Id == sectionId);
        }

        public IQueryable<Entities.DataContext.Section> GetWithPage(int sectionId)
        {
            return _context.Sections.Include(x => x.Page).Where(x => x.Id == sectionId);
        }
        public IQueryable<Entities.DataContext.Section> GetAllWithPage()
        {
            return _context.Sections.Include(x => x.Page);
        }

    }

}
