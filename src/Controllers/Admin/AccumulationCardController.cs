using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;
using JetFlight.WebApi.Controllers;
using JetFlight.Service.Validators;
using JetFlight.Shared.Models.Shared;
using JetFlight.WebApi.Authorization;
using JetFlight.Shared.Models.AccumulationCard;
using JetFlight.Shared.Models;

namespace JetFlight.WebApi.AdminControllers
{
    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class AccumulationCardController : BaseController
    {
        private readonly IAccumulationCardService _accumulationCardService;
        public AccumulationCardController(IAccumulationCardService accumulationCardService)
        {
            _accumulationCardService = accumulationCardService;
        }


        [HttpPost]
        [HasPermission(Permission.LoyaltyProgram, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(AdminAccumulationCardDTO), 200)]
        public async Task<IActionResult> Create(CreateAccumulationCardDTO model)
        {
            return Ok(await _accumulationCardService.CreateAsync(model));
        }

        [HttpPut]
        [HasPermission(Permission.LoyaltyProgram, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> Update(UpdateAccumulationCardDTO model)
        {
            await _accumulationCardService.UpdateAsync(model);
            return NoContent();
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedListDTO<AdminAccumulationCardDTO>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] AdminGetListAccumulationCardsFilterDTO filter)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(AdminAccumulationCardDTO), null, null, filter.offset, filter.limit, int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }

            var coupons = await _accumulationCardService.GetAdminCardsAsync(
                validatePagingParamsDTO.PagingDTO,
                filter.searchParam,
                filter.branchId,
                filter.cityId,
                filter.storeId,
                filter.date,
                filter.status);
            return Ok(coupons);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AdminAccumulationCardDTO), 200)]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _accumulationCardService.GetAdminCardByIdAsync(id));
        }

        [HttpPost("{id}/publish")]
        [HasPermission(Permission.LoyaltyProgram, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> Publish(int id)
        {
            await _accumulationCardService.PublishAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/archive")]
        [HasPermission(Permission.LoyaltyProgram, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> Archive(int id)
        {
            await _accumulationCardService.ArchiveAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [HasPermission(Permission.LoyaltyProgram, PermissionLevel.Delete)]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<NoContentResult> Delete(int id)
        {
            await _accumulationCardService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("customerCards/{customerId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedListDTO<CustomerAccumulationCardForAdminDTO>), 200)]
        public async Task<IActionResult> GetCustomerCardsByAdmin(int customerId,
            int? offset = null,
            int? limit = null,
            DateTime? startDateFrom = null,
            DateTime? startDateTo = null,
            DateTime? expirationDateFrom = null,
            DateTime? expirationDateTo = null,
            CustomerAccumulationCardStatusForAdmin? status = null)
        {
            RangeDTO<DateTime> startDate = null;
            RangeDTO<DateTime> expirationDate = null;

            if (startDateFrom.HasValue || startDateTo.HasValue)
            {
                startDate = new RangeDTO<DateTime>
                {
                    From = startDateFrom ?? DateTime.MinValue,
                    To = startDateTo ?? DateTime.MaxValue,
                };
            }

            if (expirationDateFrom.HasValue || expirationDateTo.HasValue)
            {
                expirationDate = new RangeDTO<DateTime>
                {
                    From = expirationDateFrom ?? DateTime.MinValue,
                    To = expirationDateTo ?? DateTime.MaxValue,
                };
            }

            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(CustomerAccumulationCardForAdminDTO), null, null, offset, limit, int.MaxValue);

            return Ok(await _accumulationCardService.GetCustomerCardsByAdminAsync(customerId, validatePagingParamsDTO.PagingDTO, startDate, expirationDate, status));
        }
    }
}
