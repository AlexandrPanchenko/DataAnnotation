using Microsoft.AspNetCore.Mvc;
using JetFlight.Service.Services;
using JetFlight.Service.Validators;
using JetFlight.Shared.Models.Posts;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Store;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.Models.Shared;

namespace JetFlight.WebApi.Controllers
{
    [ApiController]
  [ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
  [Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
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

    [HttpGet("[action]/published")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedListDTO<GetPostFullResponse>), 200)]
    public async Task<IActionResult> GetPosts([FromHeader][Required] Branches branchId, string? searchParam = null, string? orderBy = null, string? orderByDirection = null, int? offset = null, int? limit = null)
    {
      var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(typeof(GetPostFullResponse),
          orderBy,
          orderByDirection,
          offset,
          limit,
          int.MaxValue);

      if (validatePagingParamsDTO.Errors.Count != 0)
      {
        return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
      }

      return Ok(await _postsService.GetPosts(validatePagingParamsDTO.PagingDTO, searchParam, true, (int)branchId));
    }

    [HttpGet("draft/{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetPostFullResponse), 200)]
    public async Task<IActionResult> GetDraftPost(int id)
    {
      var res = await _postsService.GetPost(id);
      if (res.Id == 0)
      {
        return BadRequest(CreateErrorResponseModel(["Записів не знайдено"]));
      }
      return Ok(res);

    }

    [HttpGet("[action]")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetPostsResponse), 200)]
    public async Task<IActionResult> GetSimilarPosts([FromHeader][Required] Branches branchId, int id)
    {
      return Ok(await _postsService.GetSimilarPosts(id, (byte) branchId));
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