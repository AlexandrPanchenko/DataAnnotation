using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface IContactUsRepository : IGenericDataRepository<ContactUs>
{
    IQueryable<Entities.DataContext.ContactUs> GetAllWithImages();
}

public class ContactUsRepository : DataGenericRepository<ContactUs>, IContactUsRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public ContactUsRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }

    public IQueryable<Entities.DataContext.ContactUs> GetAllWithImages()
    {
        return _context.ContactUs.Include(x => x.Attachments);
    }
}
