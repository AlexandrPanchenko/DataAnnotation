using JetFlight.Service.Services;
using JetFlight.Shared.Models.Store;
using JetFlight.WebApi.Authorization;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace JetFlight.WebApi.Controllers.Client
{
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
    [Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
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
        public async Task<IActionResult> GetAll([FromHeader][Required] Branches branchId)
        {
            var branch = await _settingsService.GetSiteSettings((byte)branchId);
            return Ok(branch);
        }
    }
}
