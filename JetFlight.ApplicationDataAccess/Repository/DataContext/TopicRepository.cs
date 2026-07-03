using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface ITopicRepository : IGenericDataRepository<Topic>
{
}

public class TopicRepository : DataGenericRepository<Topic>, ITopicRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public TopicRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }
}
