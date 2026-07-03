using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.Models.Shared;
using JetFlight.Service;
using JetFlight.Shared.Models.Cashdesk;

namespace JetFlight.WebApi.Controllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Cashdesk.BasePathName)]
    [Route($"v1/{RouteConstants.Cashdesk.BasePathName}/[controller]")]
    public class AuthorizationController : BaseController
    {
        private readonly IJwtUtils _jwtUtils;
        public AuthorizationController(IJwtUtils jwtUtils)
        {
            _jwtUtils = jwtUtils;
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticateResponse), 200)]
        public IActionResult Authorize(CashdeskAuthorizationDto model)
        {
            if (model.ClientId != Environment.GetEnvironmentVariable("CASHDESK_CLIENT_ID") || model.ClientSecret != Environment.GetEnvironmentVariable("CASHDESK_CLIENT_SECRET"))
            {
                return BadRequest();
            }

            var token = _jwtUtils.GenerateJwtToken(model.ClientId);
            var response = new AuthenticateResponse(token);
            return Ok(response);
        }
    }
}
