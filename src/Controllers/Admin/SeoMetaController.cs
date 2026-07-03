using JetFlight.Service.Services;
using JetFlight.Shared.Models.LogHistory;
using Microsoft.AspNetCore.Mvc;
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
    public class SeoMetaController : BaseController
    {
        private readonly ISeoMetaService _seoMetaService;
        public SeoMetaController(ISeoMetaService seoMetaService)
        {
            _seoMetaService = seoMetaService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SeoMetaDTO), 200)]
        public async Task<IActionResult> GetSeoMeta(string entityType, int entityId)
        {
            var seoMeta = await _seoMetaService.Get(entityType, entityId);
            return Ok(seoMeta);
        }

        [HttpPost("update")]
        [HasPermission(Permission.Content, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(SeoMetaDTO), 200)]
        public async Task<IActionResult> Update(SeoMetaDTO seoMeta)
        {
            var updatedSeo = await _seoMetaService.UpdateSeoMeta(seoMeta);

            return Ok(updatedSeo);
        }
    }
}
