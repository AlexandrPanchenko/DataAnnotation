using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Coupons;
using JetFlight.Service.Validators;

namespace JetFlight.WebApi.Controllers
{
    [Authorize(Roles = UserRole.Customer)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
    [Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
    public class CouponController : BaseController
    {
        private readonly ICouponService _couponService;
        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedListDTO<AssignedCustomerCouponDTO>), 200)]
        public async Task<IActionResult> GetAllAssigned(int? offset, int? limit, int? storeId = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(AssignedCustomerCouponDTO), "Id", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }

            return Ok(await _couponService.GetAvailableCouponsByCustomerAsync(validatePagingParamsDTO.PagingDTO, storeId));
        }

        [HttpGet("{customerCouponId}")]
        [ProducesResponseType(typeof(AssignedCustomerCouponDTO), 200)]
        public async Task<IActionResult> GetAssigned(int customerCouponId)
        {
            return Ok(await _couponService.GetAvailableCouponByCustomerAsync(customerCouponId));
        }

        [HttpPost("setActive/{customerCouponId}")]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<NoContentResult> SetActivate(int customerCouponId, bool activeFlag)
        {
            await _couponService.SetActiveCustomerCouponAsync(customerCouponId, activeFlag);
            return NoContent();
        }

        [HttpPost("setAllActive")]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<NoContentResult> SetAllActive(bool activeFlag, int? storeId = null)
        {
            await _couponService.SetAllCustomerCouponsAsActiveAsync(activeFlag, storeId);
            return NoContent();
        }

        [HttpGet("count")]
        [ProducesResponseType(typeof(CustomerCouponCountDTO), 200)]
        public async Task<IActionResult> Count(int? storeId = null)
        {
            return Ok(await _couponService.CountForCustomerAsync(storeId));
        }
    }
}
