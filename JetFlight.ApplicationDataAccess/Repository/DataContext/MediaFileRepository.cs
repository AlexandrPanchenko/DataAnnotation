using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Drawing.Imaging;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface IMediaFilesRepository : IGenericDataRepository<MediaFiles>
{
}

public class MediaFilesRepository : DataGenericRepository<MediaFiles>, IMediaFilesRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public MediaFilesRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }

}
