using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using JetFlight.Shared.Models.Store;
using System.ComponentModel.DataAnnotations;
using JetFlight.Shared.Models.Subscription;
using Microsoft.AspNetCore.Authorization;

namespace JetFlight.WebApi.Controllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
    [Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
    public class SubscriptionController : BaseController
    {
        private readonly ISubscriptionService _service;
        public SubscriptionController(ISubscriptionService service)
        {
            _service = service;
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Subscribe([FromHeader][Required] Branches branchId, SubscriptionRequest request, CancellationToken cancellationToken)
        {
            await _service.SubscribeAsync(request, branchId, cancellationToken);
            return NoContent();
        }

    }
}
