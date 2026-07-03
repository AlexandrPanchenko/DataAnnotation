using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.LogHistory;
using JetFlight.Shared.Models.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace JetFlight.WebApi.Controllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
    [Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
    public class SeoMetaController : BaseController
    {
        private readonly ISeoMetaService _seoMetaService;
        private readonly IPostService _postService;
        private readonly IPageManagementService _pageManagementService;
        public SeoMetaController(ISeoMetaService seoMetaService, IPostService postService, IPageManagementService pageManagementService)
        {
            _seoMetaService = seoMetaService;
            _postService = postService;
            _pageManagementService = pageManagementService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SeoMetaDTO), 200)]
        public async Task<IActionResult> GetSeoMeta(string entityType, int entityId)
        {
            var seoMeta = await _seoMetaService.Get(entityType, entityId);
            return Ok(seoMeta);
        }


        [HttpGet("[action]")]
        [ProducesResponseType(typeof(SeoMetaDTO), 200)]
        public async Task<IActionResult> GetSeoByPageEnum([FromHeader][Required] Branches branchId, RootPage page, string entityType)
        {

            var seoMeta = await _seoMetaService.GetByEnum(entityType, page, (int)branchId);
            return Ok(seoMeta);
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(List<int>), 200)]
        public async Task<IActionResult> GetPublishedPostIds([FromHeader][Required] Branches branchId)
        {
            var result = await _postService.GetPublishedPostIds((byte) branchId);
            return Ok(result);
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(List<string>), 200)]
        public async Task<IActionResult> GetPublishedPageLinks([FromHeader][Required] Branches branchId)
        {
            var result = await _pageManagementService.GetPublishedPageLinks((byte)branchId);
            return Ok(result);
        }
    }
}
