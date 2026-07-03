using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.PageManagement;
using JetFlight.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;
using JetFlight.Shared.Extensions;
using System.ComponentModel.DataAnnotations;
using JetFlight.Shared.Models.Store;
using Microsoft.AspNetCore.Authorization;


namespace JetFlight.WebApi.Controllers
{
  [ApiController]
  [ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
  [Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
  public class PagesController : BaseController
  {
    private readonly IPageManagementService _pageManagementService;
    public PagesController(IPageManagementService pageManagementService)
    {
      _pageManagementService = pageManagementService;
    }

    [HttpGet("[action]")]
    [ProducesResponseType(typeof(PageDTO), 200)]
    [AllowAnonymous]
    public async Task<IActionResult> GetPage([FromHeader][Required] Branches branchId, RootPage page)
    {
      var pages = await _pageManagementService.GetPageByEnum(page, (int)branchId);
      return Ok(pages);
    }

    [HttpGet("[action]")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GetSectionsResponse>), 200)]
    public async Task<IActionResult> GetSections(int pageId)
    {
      var siteSettings = await _pageManagementService.GetSectionsForPage(pageId, false);
      return Ok(siteSettings);
    }

    [HttpGet("[action]")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GetSectionsResponse>), 200)]
    public async Task<IActionResult> GetPublishedSections([FromHeader][Required] Branches branchId, RootPage page)
    {
      var siteSettings = await _pageManagementService.GetPublishSectionsByPageEnum(page, (int)branchId);
      return Ok(siteSettings);
    }
  }
}
