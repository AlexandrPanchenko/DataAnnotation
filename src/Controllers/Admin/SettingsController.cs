using JetFlight.Service.Services;
using JetFlight.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;
using JetFlight.Shared.Constants;
using JetFlight.WebApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;

namespace JetFlight.WebApi.AdminControllers
{
    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class SettingsController : BaseController
    {
        private readonly ISettingsService _settingsService;
        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Dictionary<SiteSettingsKeys, string>), 200)]
        [SetCulture("uk-UA")]
        public async Task<IActionResult> GetAll(byte branchId)
        {
            var branch = await _settingsService.GetSiteSettings(branchId);
            return Ok(branch);
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateSettings(Dictionary<SiteSettingsKeys, string> settings, byte branchId)
        {
            await _settingsService.UpdateSiteSettingsAsync(settings, branchId);
            return NoContent();
        }
    }
}
