using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;
using JetFlight.Shared.Models.Shared;
using JetFlight.Service.Validators;
using JetFlight.Shared.Models.AccumulationCard;

namespace JetFlight.WebApi.Controllers
{
    [Authorize(Roles = UserRole.Customer)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
    [Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
    public class AccumulationCardController : BaseController
    {
        private readonly IAccumulationCardService _accumulationCardService;
        public AccumulationCardController(IAccumulationCardService accumulationCardService)
        {
            _accumulationCardService = accumulationCardService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedListDTO<CustomerAccumulationCardDTO>), 200)]
        public async Task<IActionResult> GetAll(int? offset, int? limit, int? storeId = null, CustomerAccumulationCardStatus? status = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(CustomerAccumulationCardDTO), null, null, offset, limit, int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }

            return Ok(await _accumulationCardService.GetCustomerCardsAsync(validatePagingParamsDTO.PagingDTO, storeId, status));
        }

        [HttpGet("{customerAccumulationCardId}")]
        [ProducesResponseType(typeof(CustomerAccumulationCardDTO), 200)]
        public async Task<IActionResult> Get(int customerAccumulationCardId)
        {
            return Ok(await _accumulationCardService.GetCustomerCardByIdAsync(customerAccumulationCardId));
        }

        [HttpPost("setActive/{customerAccumulationCardId}")]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<NoContentResult> SetActivate(int customerAccumulationCardId, bool activeFlag)
        {
            await _accumulationCardService.SetCustomerCardActiveFlagAsync(customerAccumulationCardId, activeFlag);
            return NoContent();
        }

        [HttpPost("setAllActive")]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<NoContentResult> SetAllActive(bool activeFlag, int? storeId = null)
        {
            await _accumulationCardService.SetApplicableCardsActiveFlagAsync(activeFlag, storeId);
            return NoContent();
        }

        [HttpPost("complete/{customerAccumulationCardId}")]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<NoContentResult> Complete(int customerAccumulationCardId, int rewardCouponId)
        {
            await _accumulationCardService.CompleteAsync(customerAccumulationCardId, rewardCouponId);
            return NoContent();
        }

        [HttpGet("count")]
        [ProducesResponseType(typeof(CustomerAccumulationCardCountDTO), 200)]
        public async Task<IActionResult> Count(int? storeId = null)
        {
            return Ok(await _accumulationCardService.CountForCustomerAsync(storeId));
        }
    }
}
