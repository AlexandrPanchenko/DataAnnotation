using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface IContactUsAttachmentRepository : IGenericDataRepository<ContactUsAttachment>
{
}

public class ContactUsAttachmentRepository : DataGenericRepository<ContactUsAttachment>, IContactUsAttachmentRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public ContactUsAttachmentRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }
}
