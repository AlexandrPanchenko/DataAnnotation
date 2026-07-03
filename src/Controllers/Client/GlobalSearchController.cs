using Microsoft.AspNetCore.Mvc;
using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Store;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.Models.GlobalSearch;

namespace JetFlight.WebApi.Controllers
{
  [ApiController]
  [ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
  [Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
  public class GlobalSearchController : BaseController
  {
    private readonly IGlobalSearchService _globalService;

    public GlobalSearchController(IGlobalSearchService globalService)
    {
            _globalService = globalService;
    }

        [HttpGet("[action]/published")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GlobalSearchResult), 200)]
        public async Task<IActionResult> GetSearchResult([FromHeader][Required] Branches branchId, [FromQuery] int? store = null, string? searchParam = null, CancellationToken cancellationToken = default)
        {
            return Ok(await _globalService.GetGlobalSearchResult(searchParam, (byte)branchId, store, cancellationToken));
        }
    }
}