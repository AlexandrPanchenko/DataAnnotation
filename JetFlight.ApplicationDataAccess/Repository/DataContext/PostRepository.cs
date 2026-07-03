using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface IPostRepository : IGenericDataRepository<Post>
{
    IQueryable<Post> GetPosts();
    List<Post> GetPostBySearchParam(string searchParam);
}

public class PostRepository : DataGenericRepository<Post>, IPostRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public PostRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }

    public IQueryable<Post> GetPosts()
    {
        return _dbContext.Posts.Include(x => x.Origin).Where(x=>x.isActive == true).AsQueryable();
    }

    public List<Post> GetPostBySearchParam(string searchParam)
    {
        var resultsByName =  _context.Posts.Include(x => x.Origin).Where(x=>x.isActive == true).ToList()
            .Where(p => p.Name.Contains(searchParam, StringComparison.OrdinalIgnoreCase));

        // If no results, search by description
        if (!resultsByName.Any())
        {
            var resultsByDescription = _context.Posts.Include(x => x.Origin).ToList().Where(item => item.Text.Contains(searchParam, StringComparison.OrdinalIgnoreCase));
            return resultsByDescription.ToList();
        }
        return resultsByName.ToList();
    }

}
