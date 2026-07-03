using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;
using JetFlight.Shared.Models.Feedback;
using JetFlight.Shared.Models;
using JetFlight.WebApi.Controllers;
using JetFlight.Service.Validators;
using JetFlight.Shared.Models.ContactUs;
using JetFlight.Shared.Models.Shared;
using JetFlight.WebApi.Authorization;

namespace JetFlight.WebApi.AdminControllers
{
    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class FeedbackController : BaseController
    {
        private readonly IFeedbackService _feedbackService;
        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedListDTO<FeedbackDTO>), 200)]
        public async Task<IActionResult> GetAll(byte? branchId = null, int? offset = null, int? limit = null, FeedbackType? type = null, FeedbackStatus? status = null, ClientPlatform? platfrom = null, DateTime? createdTime = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(FeedbackDTO), "CreatedAt", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }

            var feedbacks = await _feedbackService.GetAllAsync(validatePagingParamsDTO.PagingDTO, branchId, type, status, platfrom, createdTime);
            return Ok(feedbacks);
        }

        [HttpPost("[action]")]
        [HasPermission(Permission.Applications, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(ContactUsUpdateResponse), 204)]
        public async Task<IActionResult> ProcessClientRequest(ProcessFeedbackRequest model)
        {
            await _feedbackService.ProcessAsync(model);
            return NoContent();
        }

        [HttpPost("{id}/statusUpdate")]
        [HasPermission(Permission.Applications, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        [Authorize]
        public async Task<IActionResult> ChangeStatusAndAssignee(int id, FeedbackStatus? status = null, int? assigneeId = null)
        {
            await _feedbackService.ChangeStatusAndAssigneeAsync(id, status, assigneeId);
            return NoContent();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(FeedbackDTO), 200)]
        public async Task<IActionResult> Get(int id)
        {
            var feedback = await _feedbackService.GetAsync(id);
            return Ok(feedback);
        }
    }
}
