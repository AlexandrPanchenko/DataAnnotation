using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.Shared;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace JetFlight.ApplicationDataAccess.Repository.DataContext;

public interface IPostToTagRepository : IGenericDataRepository<PostToTag>
{
    public IQueryable<PostTag> GetPostsTag(int id);
    public void UpdatePostTags(int postId, List<int> tagsIds);
    public IQueryable<PostToTag> GetPostToTag(int id);
    public Task<PostTag> GetCategory(int id);
}

public class PostToTagRepository : DataGenericRepository<PostToTag>, IPostToTagRepository
{

    public readonly ApplicationDataAccess.ApplicationDataContext _dbContext;

    public PostToTagRepository(ApplicationDataAccess.ApplicationDataContext context) : base(context)
    {
        _dbContext = context;
    }
    public IQueryable<PostTag> GetPostsTag(int id)
    {
        return _dbContext.PostTag.Include(x => x.Category).Include(x => x.Post).Where(x => x.PostId == id).Select(x => x.Category);
    }

    public async Task<PostTag> GetCategory(int id)
    {
        return await _dbContext.PostsTags.FirstOrDefaultAsync(x => x.Id == id);
    }

    public IQueryable<PostToTag> GetPostToTag(int id)
    {
        return _dbContext.PostTag.Include(x => x.Category).Include(x => x.Post).Where(x => x.PostId == id);
    }

    public void UpdatePostTags(int postId, List<int> tagsIds)
    {
        var exitingTags = _dbContext.PostTag.Where(p => p.PostId == postId).Select(p => p.CategoryId).ToList();
        tagsIds.Except(exitingTags).ToList().ForEach(id => _dbContext.PostTag.Add(new PostToTag()
        {
            PostId = postId,
            CategoryId = id
        }));
        exitingTags.Except(tagsIds)
            .ToList()
            .ForEach(id => _dbContext.Remove(_dbContext.PostTag
                .Where(p => p.PostId == postId && p.CategoryId == id)
                .First()
            ));

    }

}
