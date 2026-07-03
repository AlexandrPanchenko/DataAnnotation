using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.WebApi.Controllers;
using JetFlight.Shared.Models.Shared;
using JetFlight.Service.Validators;
using JetFlight.Shared.Models.Message;
using JetFlight.Shared.UserContext;

namespace JetFlight.WebApi.AdminControllers
{
    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class TargetNotificationController : BaseController
    {
        private readonly ITargetNotificationService _targetNotificationService;

        public TargetNotificationController(ITargetNotificationService targetNotificationService)
        {
            _targetNotificationService = targetNotificationService;
        }

        [ProducesResponseType(typeof(TargetSmsMessageDTO), 200)]
        [HttpGet("targetSms/{id}")]
        public async Task<IActionResult> GetTargetSmsMessage(int id)
        {
            return Ok(await _targetNotificationService.GetTargetSmsMessageAsync(id));
        }

        [ProducesResponseType(typeof(TargetSmsMessageDTO), 200)]
        [HttpPost("targetSms")]
        public async Task<IActionResult> CreateTargetEmailMessage([FromForm] CreateTargetSmsMessageDTO model)
        {
            return Ok(await _targetNotificationService.CreateTargetSmsMessageAsync(model));
        }

        [ProducesResponseType(typeof(PagedListDTO<TargetSmsMessageDTO>), 200)]
        [HttpGet("targetSms")]
        public async Task<IActionResult> GetTargetSmsMessages(
            int? offset,
            int? limit,
            byte? branchId = null,
            TargetMessageStatus? status = null,
            MessageTheme? theme = null,
            DateOnly? date = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(TargetSmsMessageDTO), "Id", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            return Ok(await _targetNotificationService.GetTargetSmsMessagesAsync(validatePagingParamsDTO.PagingDTO, branchId, status, theme, date));
        }

        [ProducesResponseType(typeof(ScheduledCustomerSmsMessageDTO), 200)]
        [HttpGet("customerSms/{id}")]
        public async Task<IActionResult> GetCustomerSmsMessage(int id)
        {
            return Ok(await _targetNotificationService.GetTargetSmsMessageAsync(id));
        }

        [ProducesResponseType(typeof(PagedListDTO<ScheduledCustomerSmsMessageDTO>), 200)]
        [HttpGet("customerSms")]
        public async Task<IActionResult> GetCustomerSmsMessages(
            int? offset,
            int? limit,
            byte? branchId = null,
            int? targetSmsMessageId = null,
            ScheduledCustomerMessageStatus? status = null,
            MessageTheme? theme = null,
            DateOnly? date = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(ScheduledCustomerSmsMessageDTO), "Id", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            return Ok(await _targetNotificationService.GetScheduledCustomerSmsMessagesAsync(validatePagingParamsDTO.PagingDTO, branchId, targetSmsMessageId, status, theme, date));
        }

        [ProducesResponseType(typeof(string), 200)]
        [HttpPost("targetEmail/generateBody")]
        public async Task<IActionResult> GenerateEmailFromParameters([FromForm] CreateEmailParametersDTO parameters)
        {
            return Ok(await _targetNotificationService.GenerateTargetEmailBodyAsync(parameters));
        }

        [ProducesResponseType(typeof(string), 200)]
        [HttpGet("targetEmail/{id}/generateBody")]
        public async Task<IActionResult> GenerateEmailFromMessage(int id)
        {
            return Ok(await _targetNotificationService.GenerateTargetEmailBodyAsync(id));
        }

        [ProducesResponseType(typeof(TargetEmailMessageDTO), 200)]
        [HttpGet("targetEmail/{id}")]
        public async Task<IActionResult> GetTargetEmailMessage(int id)
        {
            return Ok(await _targetNotificationService.GetTargetEmailMessageAsync(id));
        }

        [ProducesResponseType(typeof(TargetEmailMessageDTO), 200)]
        [HttpPost("targetEmail")]
        public async Task<IActionResult> CreateTargetEmailMessage([FromForm] CreateTargetEmailMessageDTO model)
        {
            return Ok(await _targetNotificationService.CreateTargetEmailMessageAsync(model));
        }

        [ProducesResponseType(typeof(PagedListDTO<TargetEmailMessageDTO>), 200)]
        [HttpGet("targetEmail")]
        public async Task<IActionResult> GetTargetEmailMessages(
            int? offset,
            int? limit,
            byte? branchId = null,
            TargetMessageStatus? status = null,
            MessageTheme? theme = null,
            DateOnly? date = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(TargetEmailMessageDTO), "Id", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            return Ok(await _targetNotificationService.GetTargetEmailMessagesAsync(validatePagingParamsDTO.PagingDTO, branchId, status, theme, date));
        }

        [ProducesResponseType(typeof(ScheduledCustomerEmailMessageDTO), 200)]
        [HttpGet("customerEmail/{id}")]
        public async Task<IActionResult> GetCustomerEmailMessage(int id)
        {
            return Ok(await _targetNotificationService.GetTargetEmailMessageAsync(id));
        }

        [ProducesResponseType(typeof(PagedListDTO<ScheduledCustomerEmailMessageDTO>), 200)]
        [HttpGet("customerEmail")]
        public async Task<IActionResult> GetCustomerEmailMessages(
            int? offset,
            int? limit,
            byte? branchId = null,
            int? targetEmailMessageId = null,
            ScheduledCustomerMessageStatus? status = null,
            MessageTheme? theme = null,
            DateOnly? date = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(ScheduledCustomerEmailMessageDTO), "Id", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            return Ok(await _targetNotificationService.GetScheduledCustomerEmailMessagesAsync(validatePagingParamsDTO.PagingDTO, branchId, targetEmailMessageId, status, theme, date));
        }

        [ProducesResponseType(typeof(NoContentResult), 204)]
        [HttpPost("targetEmail/sendTestEmail")]
        public async Task<IActionResult> SendTestEmail([FromForm] PreviewEmailParametersDTO paramaters)
        {
            await _targetNotificationService.SendTestEmailAsync(paramaters);
            return new NoContentResult();
        }

        [ProducesResponseType(typeof(NoContentResult), 204)]
        [HttpPost("targetEmail/{id}/sendTestEmail")]
        public async Task<IActionResult> SendTestEmail(int id)
        {
            await _targetNotificationService.SendTestEmailAsync(id);
            return new NoContentResult();
        }
    }
}