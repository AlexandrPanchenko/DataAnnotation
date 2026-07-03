using JetFlight.Service.Services;
using JetFlight.Service.Validators;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.ContactUs;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.UserContext;
using JetFlight.WebApi.Authorization;
using JetFlight.WebApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JetFlight.WebApi.AdminControllers
{
    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class ContactUsController : BaseController
    {
        private readonly IContactUsService _contactUsService;
        public ContactUsController(IContactUsService contactUsService)
        {
            _contactUsService = contactUsService;
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedListDTO<GetContactUsResponseDTO>), 200)]
        public async Task<IActionResult> GetContactUs(int? branchId = null, int? offset = null, int? limit = null, RequestStatus? requestStatus = null, DateTime? createdTime = null, int? topicId = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(GetContactUsResponseDTO), "CreatedAt", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }

            var contactUsList = await _contactUsService.GetAll(validatePagingParamsDTO.PagingDTO, branchId, requestStatus, createdTime, topicId);
            return Ok(contactUsList);
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(List<TopicDTO>), 200)]
        public async Task<IActionResult> GetTopics()
        {
            var topics = await _contactUsService.GetAllTopics();
            return Ok(topics);
        }

        [HttpPost("[action]")]
        [HasPermission(Permission.Applications, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(ContactUsUpdateResponse), 200)]
        [Authorize]
        public async Task<IActionResult> ProcessClientRequest(ContactUsUpdateRequest contactUs)
        {
            var updatedContactUs = await _contactUsService.UpdateContactUs(contactUs);
            return Ok(updatedContactUs);
        }

        [HttpPost("statusUpdate/{contactUsId}")]
        [HasPermission(Permission.Applications, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(ContactUsChangeStatusAndAssigneResponse), 200)]
        [Authorize]
        public async Task<IActionResult> ChangeStatusAndAssignee(int contactUsId, RequestStatus? status = null, int? assigneeId = null)
        {
            var updatedContactUs = await _contactUsService.ChangeStatusAndAssignee(contactUsId, status, assigneeId);
            return Ok(updatedContactUs);
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GetContactUsResponseDTO), 200)]
        public async Task<IActionResult> GetContactUsById(int contactUsId)
        {
            var contactUs = await _contactUsService.GetById(contactUsId);
            return Ok(contactUs);
        }
    }
}

