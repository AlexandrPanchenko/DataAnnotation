using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using JetFlight.Shared.Models.Store;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;
using JetFlight.Shared.Models.Feedback;
using JetFlight.Shared.Models;

namespace JetFlight.WebApi.Controllers
{
    [Authorize(Roles = UserRole.Customer)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
    [Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
    public class FeedbackController : BaseController
    {
        private readonly IFeedbackService _feedbackService;
        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost("createForBranch")]
        [ProducesResponseType(typeof(NoContentResult), 200)]
        public async Task<IActionResult> CreateForBranch([FromHeader][Required] Branches branchId, [FromHeader][Required] ClientPlatform platform, [FromForm] CreateBranchFeedbackDTO model)
        {
            await _feedbackService.CreateForBranchAsync(model, platform, (byte)branchId);
            return NoContent();
        }

        [HttpPost("createForStore")]
        [ProducesResponseType(typeof(NoContentResult), 200)]
        public async Task<IActionResult> CreateForStore([FromHeader][Required] Branches branchId, [FromHeader][Required] ClientPlatform platform, [FromForm] CreateStoreFeedbackDTO model)
        {
            await _feedbackService.CreateForStoreAsync(model, platform, (byte)branchId);
            return NoContent();
        }
    }
}
