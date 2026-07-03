using Microsoft.AspNetCore.Mvc;
using JetFlight.Service.Services;
using JetFlight.Service.Validators;
using JetFlight.Shared.Models.Posts;
using JetFlight.Shared.Models.Shared;
using JetFlight.WebApi.Controllers;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;
using JetFlight.WebApi.Authorization;

namespace JetFlight.WebApi.AdminControllers
{
    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class PostsController : BaseController
    {
        private readonly IPostService _postsService;

        public PostsController(IPostService roleService)
        {
            _postsService = roleService;
        }


        [HttpGet("categories")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<GetPostTagResponse>), 200)]
        public async Task<IActionResult> GetCategories()
        {
            return Ok(await _postsService.GetAllPostTags());
        }

        [HttpGet("[action]/draft")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedListDTO<GetPostFullResponse>), 200)]
        public async Task<IActionResult> GetDraftPosts(string? searchParam = null, string? orderBy = null, string? orderByDirection = null, int? offset = null, int? limit = null, int? branchId = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(typeof(GetPostFullResponse),
                orderBy,
                orderByDirection,
                offset,
                limit,
                int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }
            return Ok(await _postsService.GetPosts(validatePagingParamsDTO.PagingDTO, searchParam, false, branchId));
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedListDTO<GetPostFullResponse>), 200)]
        public async Task<IActionResult> GetPublishedPosts(string? searchParam = null, string? orderBy = null, string? orderByDirection = null, int? offset = null, int? limit = null, int? branchId = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(typeof(GetPostFullResponse),
                orderBy,
                orderByDirection,
                offset,
                limit,
                int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }
            return Ok(await _postsService.GetPosts(validatePagingParamsDTO.PagingDTO, searchParam, true, branchId));
        }

        [HttpGet("draft/{id}")]
        [ProducesResponseType(typeof(GetPostFullResponse), 200)]
        [AllowAnonymous]
        public async Task<IActionResult> GetDraftPost(int id)
        {
            var res = await _postsService.GetPost(id);
            if (res.Id == 0)
            {
                return BadRequest(CreateErrorResponseModel(["Записів не знайдено"]));
            }
            return Ok(res);

        }

        [HttpPost("create")]
        [HasPermission(Permission.Content, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(PostCreateResponse), 200)]
        public async Task<IActionResult> Create([FromForm] PostCreateRequest post)
        {
            var newPost = await _postsService.CreatePost(post);
            if (newPost.Errors.Any())
            {
                return BadRequest(CreateErrorResponseModel(newPost.Errors));
            }
            return Ok(newPost);
        }

        [HttpPost("update")]
        [HasPermission(Permission.Content, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(PostUpdateResponse), 200)]
        public async Task<IActionResult> Update([FromForm] PostUpdateRequest post)
        {
            var updatedPost = await _postsService.UpdatePost(post);
            if (updatedPost.Errors.Any())
            {
                return BadRequest(CreateErrorResponseModel(updatedPost.Errors));
            }
            return Ok(updatedPost);
        }

        [HttpDelete("delete")]
        [HasPermission(Permission.Content, PermissionLevel.Delete)]
        [ProducesResponseType(typeof(DeleteResponseDTO), 200)]
        public async Task<IActionResult> Delete(int postId)
        {
            var result = await _postsService.DeletePost(postId);
            if (result.Errors.Any())
            {
                return BadRequest(CreateErrorResponseModel(result.Errors));
            }
            return Ok(result);
        }


        [HttpGet("published/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GetPostFullResponse), 200)]
        public async Task<IActionResult> GetPost(int id)
        {
            var res = await _postsService.GetPublishPost(id);
            if (res.Id == 0)
            {
                return BadRequest(CreateErrorResponseModel(["Записів не знайдено"]));
            }
            return Ok(res);
        }
    }
}