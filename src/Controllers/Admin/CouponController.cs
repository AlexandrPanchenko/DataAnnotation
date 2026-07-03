using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;
using JetFlight.WebApi.Controllers;
using JetFlight.Service.Validators;
using JetFlight.Shared.Models.Shared;
using JetFlight.WebApi.Authorization;
using JetFlight.Shared.Models.Coupons;
using JetFlight.Shared.Models;

namespace JetFlight.WebApi.AdminControllers
{
    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class CouponController : BaseController
    {
        private readonly ICouponService _couponService;
        private readonly IMediaService _mediaService;
        public CouponController(ICouponService couponService, IMediaService mediaService)
        {
            _couponService = couponService;
            _mediaService = mediaService;
        }


        [HttpPost]
        [HasPermission(Permission.LoyaltyProgram, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(AdminCouponDTO), 200)]
        public async Task<IActionResult> Create(CreateCouponDTO model)
        {
            return Ok(await _couponService.CreateAsync(model));
        }

        [HttpPut]
        [HasPermission(Permission.LoyaltyProgram, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> Update(UpdateCouponDTO model)
        {
            await _couponService.UpdateAsync(model);
            return NoContent();
        }

        [Obsolete]
        [HttpPost("uploadImage")]
        public async Task<Uri> UploadImage(IFormFile formFile)
        {
            return await _mediaService.UploadAsync(formFile);
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedListDTO<AdminCouponDTO>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] AdminCouponFilterDTO filter)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(AdminCouponDTO), "CreatedAt", OrderByDirectionTypes.DESC.ToString(), filter.offset, filter.limit, int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }

            var coupons = await _couponService.GetCouponsByAdminAsync(validatePagingParamsDTO.PagingDTO, filter);
            return Ok(coupons);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AdminCouponDTO), 200)]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _couponService.GetByAdminAsync(id));
        }

        [HttpPost("{id}/publish")]
        [HasPermission(Permission.LoyaltyProgram, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> Publish(int id)
        {
            await _couponService.PublishAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/archive")]
        [HasPermission(Permission.LoyaltyProgram, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> Archive(int id)
        {
            await _couponService.ArchiveAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [HasPermission(Permission.LoyaltyProgram, PermissionLevel.Delete)]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<NoContentResult> Delete(int id)
        {
            await _couponService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("customerCoupons/{customerId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedListDTO<CustomerCouponForAdminDTO>), 200)]
        public async Task<IActionResult> GetCustomerCouponsByAdmin(int customerId,
            int? offset = null,
            int? limit = null,
            DateTime? startDateFrom = null,
            DateTime? startDateTo = null,
            DateTime? expirationDateFrom = null,
            DateTime? expirationDateTo = null,
            CustomerCouponStatus? status = null)
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
                typeof(CustomerCouponForAdminDTO), "Id", OrderByDirectionTypes.ASC.ToString(), offset, limit, int.MaxValue);

            return Ok(await _couponService.GetCustomerCouponsByAdminAsync(customerId, validatePagingParamsDTO.PagingDTO, startDate, expirationDate, status));
        }
    }
}
