using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.ApplicationDataAccess.Repository.DataContext;
using JetFlight.Service.Extensions;
using JetFlight.Shared;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Posts;
using JetFlight.Shared.Models.Shared;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace JetFlight.Service.Services
{
    public interface IPostService
    {
        Task<GetPostFullResponse> GetPost(int id);
        Task<List<GetPostTagResponse>> GetAllPostTags();

        Task<GetPostsResponse> GetSimilarPosts(int id, byte branchId);
        Task<PagedListDTO<GetPostFullResponse>> GetPosts(PagingDTO pagingDto, string? searchParam = null, bool published = false, int? branchId = null);
        Task<PostCreateResponse> CreatePost(PostCreateRequest post);
        Task<PostUpdateResponse> UpdatePost(PostUpdateRequest post);
        Task<DeleteResponseDTO> DeletePost(int postId);
        Task<GetPostFullResponse> GetPublishPost(int id);
        Task<List<Post>> GetAllPosts();
        Task<List<int>> GetPublishedPostIds(byte branchId);
    }
    public class PostService : IPostService
    {
        private readonly IDataUnitOfWork _unitOfWork;

        private readonly IGlobalSearchService _postSearchService;

        private readonly IMediaService _mediaService;

        public PostService(IDataUnitOfWork unitOfWork, IGlobalSearchService postSearchService, IMediaService mediaService)
        {
            _unitOfWork = unitOfWork;
            _postSearchService = postSearchService;
            _mediaService = mediaService;
        }

        public async Task<GetPostFullResponse> GetPost(int id)
        {
            var post = await _unitOfWork.Posts.GetPosts().Where(x => x.Id == id && x.Status == false).FirstOrDefaultAsync();
            if (post == null) return new GetPostFullResponse();
            var posTags = new GetPostFullResponse
            {
                Id = post.Id,
                Name = post.Name,
                CreatedAt = post.CreatedAt,
                Subtitle = post.Subtitle,
                Text = post.Text,
                UpdatedAt = post.UpdatedAt,
                PublishedAt = post.PublishedAt,
                Status = post.Origin != null
                    ? post.Origin.Status != null && post.Origin.Status.Value
                    : post.Status != null && post.Status.Value,
                BranchId = post.BranchId,
                ImagePath = post.ImageName != null ? new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!) { Path = $"{StorageConstants.AppPath}/{post.ImageName}" }.ToString() : "",
                ImageSize = post.ImageSize,
                ImageMimeType = post.ImageMimeType,
                ImageAlt = post.ImageAlt,


                PostTags = _unitOfWork.PostToTags.GetPostsTag(post.Id).Select(x => new GetPostTagResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    CreatedAt = x.CreatedAt != null ? x.CreatedAt.Value : DateTime.UtcNow,
                    UpdatedAt = x.UpdatedAt,
                    Icon = x.Icon,
                    Name = x.Name
                }).ToList()
            };

            return posTags;
        }

        public async Task<GetPostFullResponse> GetPublishPost(int id)
        {
            Post? post = null;
            post = await _unitOfWork.Posts.GetPosts().Where(x => x.Id == id && x.Origin != null).Select(p => p.Origin).FirstOrDefaultAsync();
            if (post == null)
            {
                post = await _unitOfWork.Posts.GetPosts().Where(x => x.Id == id && x.Status == true).FirstOrDefaultAsync();
            }
            if (post == null) return new GetPostFullResponse();

            var posTags = new GetPostFullResponse
            {
                Id = post.Id,
                Name = post.Name,
                CreatedAt = post.CreatedAt,
                Subtitle = post.Subtitle,
                Text = post.Text,
                UpdatedAt = post.UpdatedAt,
                PublishedAt = post.PublishedAt,
                Status = post.Status != null && post.Status.Value,
                BranchId = post.BranchId,
                ImagePath = post.ImageName != null ? new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!) { Path = $"{StorageConstants.AppPath}/{post.ImageName}" }.ToString() : "",
                ImageSize = post.ImageSize,
                ImageMimeType = post.ImageMimeType,
                ImageAlt = post.ImageAlt,


                PostTags = _unitOfWork.PostToTags.GetPostsTag(post.Id).Select(x => new GetPostTagResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    CreatedAt = x.CreatedAt != null ? x.CreatedAt.Value : DateTime.UtcNow,
                    UpdatedAt = x.UpdatedAt,
                    Icon = x.Icon,
                    Name = x.Name
                }).ToList()
            };

            return posTags;
        }
        public async Task<PagedListDTO<GetPostFullResponse>> GetPosts(PagingDTO pagingDto, string? searchParam = null, bool published = false, int? branchId = null)
        {
            if (published == true)
            {
                var publishedTags = _unitOfWork.Posts.GetAll().Where(x => x.Origin == null && x.Status == true && x.PublishedAt.HasValue).OrderByDescending(x => x.CreatedAt).AsQueryable();

                if (branchId.HasValue)
                {
                    publishedTags = publishedTags.Where(x => x.BranchId == null || x.BranchId == branchId);
                }

                if (!string.IsNullOrWhiteSpace(searchParam))
                {
                    var trimmed = searchParam.Trim();
                    var pattern = $"%{trimmed.ToLower()}%";
                    publishedTags = publishedTags.Where(p =>
                        (p.Name != null && EF.Functions.Like(p.Name.ToLower(), pattern)) ||
                        (p.Subtitle != null && EF.Functions.Like(p.Subtitle.ToLower(), pattern)) ||
                        (p.Text != null && EF.Functions.Like(p.Text.ToLower(), pattern)));
                }

                var publishedTagsDTO = await publishedTags.GetPagedListAsync(pagingDto, ToDTO);

                return publishedTagsDTO;
            }

            var postTags =  _unitOfWork.Posts.GetPosts().Where(x => x.Status == false).OrderByDescending(x => x.CreatedAt).AsQueryable();


            if (branchId.HasValue)
            {
                postTags = postTags.Where(x => x.BranchId == null || x.BranchId == branchId);
            }

            var postTagsDTO = await postTags.GetPagedListAsync(pagingDto, ToDTO);
            return postTagsDTO;

        }

        private GetPostFullResponse ToDTO(Post post)
        {
            return new GetPostFullResponse
            {
                Id = post.Id,
                Name = post.Name,
                CreatedAt = post.CreatedAt,
                Subtitle = post.Subtitle,
                Text = post.Text,
                UpdatedAt = post.UpdatedAt,
                PublishedAt = post.PublishedAt,
                ImageAlt = post.ImageAlt,
                Status = post.Origin != null
                ? post.Origin.Status != null && post.Origin.Status.Value
                        : post.Status != null && post.Status.Value,
                BranchId = post.BranchId,
                ImagePath = post.ImageName != null ? new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!) { Path = $"{StorageConstants.AppPath}/{post.ImageName}" }.ToString() : "",
                PostTags = _unitOfWork.PostToTags.GetPostsTag(post.Id).Select(x => new GetPostTagResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    CreatedAt = x.CreatedAt != null ? x.CreatedAt.Value : DateTime.UtcNow,
                    UpdatedAt = x.UpdatedAt,
                    Icon = x.Icon,
                    Name = x.Name
                }).ToList()
            };
        }

        public async Task<PostCreateResponse> CreatePost(PostCreateRequest post)
        {
            var response = new PostCreateResponse();
            if (post.file == null || post.file.Length == 0)
            {
                response.Errors.Add("Оберіть картинку для завантаження");
                return response;
            }

            var newFilePath = await _mediaService.UploadAsync(post.file);
            var newFileName = Path.GetFileName(newFilePath.ToString());

            var request = new Post
            {
                Name = post.name,
                CreatedAt = DateTime.UtcNow,
                Subtitle = post.subtitle,
                Text = post.text,
                PublishedAt = post.publishedAt,
                Published = false,
                Status = post.status,
                BranchId = post.branchId,
                isActive = true,
                ImageMimeType = post.file.ContentType,
                ImageSize = post.file!.Length.ToString(),
                ImageName = newFileName,
                ImageAlt = post.imageAlt
            };
            var result = await _unitOfWork.Posts.Add(request);
            await _unitOfWork.Save();

            if (post.postTags != null && !post.postTags.Any(x => x == 0) && post.postTags.Count > 0)
            {
                _unitOfWork.PostToTags.UpdatePostTags(result.Id, post.postTags.Select(m => m).ToList());
                await _unitOfWork.Save();
            }

            response.Item = new GetPostResponse()
            {
                Name = result.Name,
                CreatedAt = result.CreatedAt,
                Subtitle = result.Subtitle,
                Text = result.Text,
                PublishedAt = result.PublishedAt,
                Status = result.Status.HasValue ? result.Status.Value : false,
                BranchId = result.BranchId,
                UpdatedAt = result.UpdatedAt,

                ImageMimeType = result.ImageMimeType,
                ImageSize = result.ImageSize,
                ImagePath = result.ImageName ?? string.Empty,
                ImageAlt = result.ImageAlt,
                Id = result.Id
            };

            return response;
        }

        public async Task<DeleteResponseDTO> DeletePost(int postId)
        {
            var postResponseDto = new DeleteResponseDTO();
            var postToRemove = await _unitOfWork.Posts.GetById(postId);
            if (postToRemove == null || postToRemove.isActive == false)
            {
                postResponseDto.Errors.Add("Стаття не знайдена");
                return postResponseDto;
            }

            var postsToTagsToRemove = _unitOfWork.PostToTags.GetAll().Where(x => x.PostId == postId).ToList();
            postsToTagsToRemove.ToList().ForEach(posttag => _unitOfWork.PostToTags.Remove(posttag));

            //remove published version if exist
            if (postToRemove.OriginId != null)
            {
                var p = await _unitOfWork.Posts.GetAll().Where(x => x.Id == postToRemove.OriginId).FirstOrDefaultAsync();

                if (p != null)
                {
                    var settingsToReset = await _unitOfWork.SiteSettings.Find(x => x.Key == SiteSettingsKeys.fixed_post_id && x.Value == p.Id.ToString()).ToListAsync();
                    foreach (var setting in settingsToReset)
                    {
                        setting.Value = null;
                    }
                }

                var publishedPostsToTagsToRemove = _unitOfWork.PostToTags.GetAll().Where(x => x.PostId == postToRemove.OriginId).ToList();
                if (publishedPostsToTagsToRemove != null) publishedPostsToTagsToRemove.ToList().ForEach(posttag => _unitOfWork.PostToTags.Remove(posttag));

                if (p != null)
                {
                    _unitOfWork.Posts.Remove(p);
                    await _unitOfWork.Save();
                }
            }

            _unitOfWork.Posts.Remove(postToRemove);
            await _unitOfWork.Save();

            postResponseDto.Result = true;

            return postResponseDto;
        }

        public async Task<PostUpdateResponse> UpdatePost(PostUpdateRequest post)
        {
            var result = await _unitOfWork.Posts.GetPosts().Where(x => x.Id == post.id).FirstOrDefaultAsync();
            var response = new PostUpdateResponse();

            if (result != null)
            {
                var dateTimeNow = DateTime.UtcNow.SetKindUtc();
                if (!string.IsNullOrEmpty(post.name)) result.Name = post.name;
                if (!string.IsNullOrEmpty(post.subtitle)) result.Subtitle = post.subtitle;
                if (post.readDurationMin != result.ReadDurationMin) result.ReadDurationMin = post.readDurationMin;
                if (post.imageAlt != result.ImageAlt && post.imageAlt != null) result.ImageAlt = post.imageAlt;
                if (post.text != result.Text && post.text != null) result.Text = post.text;
                result.BranchId = post.branchId;
                result.UpdatedAt = dateTimeNow;
                if (post.postTags != null && post.postTags.Any(x => x != null && x.Value == 0))
                {
                    var postsToTagsToRemove = _unitOfWork.PostToTags.GetAll().Where(x => x.PostId == result.Id).ToList();
                    postsToTagsToRemove.ToList().ForEach(_unitOfWork.PostToTags.Remove);
                }
                if (post.postTags != null && !post.postTags.Any(x => x != null && x.Value == 0) && post.postTags.Count > 0)
                {
                    _unitOfWork.PostToTags.UpdatePostTags(
                        result.Id,
                        post.postTags.Where(m => m.HasValue).Select(m => m!.Value).ToList());
                    await _unitOfWork.Save();
                }


                if (post.file != null)
                {
                    if (post.file.Length == 0)
                    {
                        response.Errors.Add("Оберіть картинку для завантаження");
                    }
                    else
                    {

                        var newFilePath = await _mediaService.UploadAsync(post.file);
                        var newFileName = Path.GetFileName(newFilePath.ToString());

                        result.ImageMimeType = post.file.ContentType;
                        result.ImageSize = post.file.Length.ToString();
                        result.ImageName = newFileName;
                        result.ImageAlt = post.imageAlt;
                    }
                }
                await _unitOfWork.Save();

                if (post.status == true)
                {
                    if (result.Origin != null)
                    {
                        result.Origin.Name = result.Name;
                        result.Origin.Subtitle = result.Subtitle;
                        result.Origin.ReadDurationMin = result.ReadDurationMin;
                        result.Origin.Text = result.Text;
                        result.Origin.Status = true;
                        result.Origin.UpdatedAt = dateTimeNow;
                        result.Origin.isActive = result.isActive;
                        result.Origin.ImageAlt = result.ImageAlt;
                        result.Origin.PublishedAt = dateTimeNow;
                        result.Origin.BranchId = result.BranchId;
                        if (result.ImageName != null)
                        {
                            result.Origin.ImageMimeType = result.ImageMimeType;
                            result.Origin.ImageSize = result.ImageSize;
                            result.Origin.ImageName = result.ImageName;
                        }
                        if (post.postTags != null && !post.postTags.Any(x => x != null && x.Value == 0) && post.postTags.Count > 0)
                        {
                            _unitOfWork.PostToTags.UpdatePostTags(
                                result.Origin.Id,
                                post.postTags.Select(m => m!.Value).ToList()
                            );
                            await _unitOfWork.Save(true);
                        }
                        else if (post.postTags != null && post.postTags.Count == 0)
                        {
                            var categoriesId = await _unitOfWork.PostToTags.GetAll().Where(x => x.Id == post.id).Select(x => x.CategoryId).ToListAsync();
                            if (categoriesId != null)
                            {
                                _unitOfWork.PostToTags.UpdatePostTags(result.Origin.Id, categoriesId);
                                await _unitOfWork.Save(true);
                            }
                        }
                        result.PublishedAt = dateTimeNow;
                    }
                    else
                    {
                        var published = await _unitOfWork.Posts.GetById(post.id);
                        var request = await _unitOfWork.Posts.Add(new ApplicationDataAccess.Entities.DataContext.Post
                        {
                            Name = published.Name,
                            CreatedAt = DateTime.UtcNow,
                            ReadDurationMin = published.ReadDurationMin,
                            Subtitle = published.Subtitle,
                            Text = published.Text,
                            PublishedAt = dateTimeNow,
                            Status = true,
                            BranchId = published.BranchId,
                            UpdatedAt = dateTimeNow,
                            isActive = published.isActive,
                            ImageMimeType = published.ImageMimeType,
                            ImageSize = published.ImageSize,
                            ImageName = published.ImageName,
                            ImageAlt = published.ImageAlt

                        });
                        await _unitOfWork.Save(true);

                        if (post.postTags != null && !post.postTags.Any(x => x != null && x.Value == 0) && post.postTags.Count > 0)
                        {
                            _unitOfWork.PostToTags.UpdatePostTags(request.Id, post.postTags.Select(m => m!.Value).ToList());
                        }
                        else if (post.postTags != null && post.postTags.Count == 0)
                        {
                            var categoriesId = await _unitOfWork.PostToTags.GetAll().Where(x => x.Id == post.id).Select(x => x.CategoryId).ToListAsync();
                            if (categoriesId != null)
                            {
                                _unitOfWork.PostToTags.UpdatePostTags(request.Id, categoriesId);
                                await _unitOfWork.Save(true);
                            }
                        }

                        result.PublishedAt = dateTimeNow;

                        result.OriginId = request.Id;
                        result.Status = false;

                    }
                }
                else if (post.status == false)
                {
                    if (result.Origin != null)
                    {
                        var postsToTagsToRemove = _unitOfWork.PostToTags.GetAll().Where(x => x.PostId == result.Origin.Id).ToList();
                        postsToTagsToRemove.ToList().ForEach(posttag => _unitOfWork.PostToTags.Remove(posttag));

                        //remove published version if exist
                        var p = await _unitOfWork.Posts.GetAll().Where(x => x.Id == result.Origin.Id).FirstOrDefaultAsync();

                        if (p != null)
                        {
                            var settingsToReset = await _unitOfWork.SiteSettings.Find(x => x.Key == SiteSettingsKeys.fixed_post_id && x.Value == p.Id.ToString()).ToListAsync();
                            foreach (var setting in settingsToReset)
                            {
                                setting.Value = null;
                            }

                            _unitOfWork.Posts.Remove(p);
                        }

                        result.Origin = null;
                        result.PublishedAt = null;
                        await _unitOfWork.Save();
                    }
                }
                result.UpdatedAt = dateTimeNow;

            }

            if (response.Errors.Count == 0 && result != null)
            {
                await _unitOfWork.Save();
                response.Item = new GetPostResponse()
                {
                    Name = result.Name,
                    CreatedAt = result.CreatedAt,
                    Subtitle = result.Subtitle,
                    Text = result.Text,
                    PublishedAt = result.PublishedAt,
                    Status = result.Status.HasValue ? result.Status.Value : false,
                    BranchId = result.BranchId,
                    UpdatedAt = result.UpdatedAt,

                    ImageMimeType = result.ImageMimeType,
                    ImageSize = result.ImageSize,
                    ImagePath = result.ImageName ?? string.Empty,
                    ImageAlt = result.ImageAlt ?? string.Empty,
                    Id = result.Id
                };

            }

            return response;
        }

        public async Task<List<GetPostTagResponse>> GetAllPostTags()
        {
            var tags = await _unitOfWork.Tags.GetAll().ToListAsync();

            var rolesResponse = tags.ToList()
                .Select(tag => new GetPostTagResponse
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    Title = tag.Title,
                    Icon = tag.Icon,
                    CreatedAt = tag.CreatedAt != null ? tag.CreatedAt.Value : DateTime.UtcNow,
                    UpdatedAt = tag.UpdatedAt

                }).ToList();

            return rolesResponse;
        }

        public async Task<GetPostsResponse> GetSimilarPosts(int id, byte branchId)
        {
            var originalPosts = await _unitOfWork.Posts.GetAll().Where(x => x.Id == id && x.isActive == true).FirstOrDefaultAsync();
            var tagsIds = await _unitOfWork.PostToTags.GetPostToTag(id).Select(pt => pt.CategoryId).ToListAsync();

            if (originalPosts == null)
            {
                throw new ArgumentException("Стаття не знайдена");
            }

            if (originalPosts.BranchId != null && originalPosts.BranchId != branchId)
            {
                throw new ArgumentException("Стаття не відноситься для обраного сайту");
            }

            var similarPosts = await _unitOfWork.Posts.GetAll()
                .Where(x => (x.BranchId == null || x.BranchId == branchId)
                    && x.PostTags.Any(pt => tagsIds.Contains(pt.CategoryId)) && x.Id != id && x.Status == true && x.Id != originalPosts.OriginId)
                .ToListAsync();
            if (similarPosts.Count < 10)
            {
                var otherPosts = await _unitOfWork.Posts.GetAll()
                    .Where(x => x.Id != id
                        && (x.BranchId == null || x.BranchId == branchId)
                        && x.Status == true && x.Id != originalPosts.OriginId)
                    .ToListAsync();
                foreach (var item in otherPosts.Take(10 - similarPosts.Count))
                {
                    if (!similarPosts.Any(it => it.Id == item.Id))
                        similarPosts.Add(item);
                }
            }

            var result = similarPosts.Select(post => new GetPostFullResponse
            {
                Id = post.Id,
                Name = post.Name,
                CreatedAt = post.CreatedAt,
                Subtitle = post.Subtitle,
                Text = post.Text,
                UpdatedAt = post.UpdatedAt,
                PublishedAt = post.PublishedAt,
                ImageAlt = post.ImageAlt,
                Status = post.Status != null && post.Status.Value,
                BranchId = post.BranchId,
                ImagePath = post.ImageName != null ? new UriBuilder(Environment.GetEnvironmentVariable("API_URL")!) { Path = $"{StorageConstants.AppPath}/{post.ImageName}" }.ToString() : "",
                PostTags = _unitOfWork.PostToTags.GetPostsTag(post.Id).Select(x => new GetPostTagResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    CreatedAt = x.CreatedAt != null ? x.CreatedAt.Value : DateTime.UtcNow,
                    UpdatedAt = x.UpdatedAt,
                    Icon = x.Icon,
                    Name = x.Name
                }).Take(10).ToList()
            });

            var response = new GetPostsResponse
            {
                Total = result.Count(),
                Posts = result.ToList()
            };

            return response;
        }

        public async Task<List<Post>> GetAllPosts()
        {
            var posts = await _unitOfWork.Posts.GetPosts().ToListAsync();
            return posts;
        }

        public Task<List<int>> GetPublishedPostIds(byte branchId)
            => _unitOfWork.Posts.GetPosts()
            .Where(x => x.Status == true && x.OriginId == null && (x.BranchId == null || x.BranchId == branchId))
            .Select(x => x.Id).ToListAsync();
    }
}
