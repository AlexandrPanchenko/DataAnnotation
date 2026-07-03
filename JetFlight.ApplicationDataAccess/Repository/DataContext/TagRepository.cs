using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface ITagRepository : IGenericDataRepository<PostTag>
{
}

public class TagRepository : DataGenericRepository<PostTag>, ITagRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public TagRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }
}
