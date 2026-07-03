using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.WebApi.Controllers;
using JetFlight.Shared.Models.RFM;
using JetFlight.Shared.Models.Shared;
using JetFlight.Service.Validators;
using JetFlight.Shared.Models;
using JetFlight.Shared.UserContext;

namespace JetFlight.WebApi.AdminControllers
{

    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class RFMController : BaseController
    {
        private readonly IRFMService _RFMService;
        public RFMController(IRFMService RFMService)
        {
            _RFMService = RFMService;
        }


        [ProducesResponseType(typeof(RfmDto), 200)]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _RFMService.GetAsync(id));
        }

        [ProducesResponseType(typeof(NoContentResult), 204)]
        [HttpPut]
        public async Task<IActionResult> Update(RfmDto model)
        {
            await _RFMService.UpdateAsync(model);
            return NoContent();
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(List<RfmDto>), 200)]
        public async Task<ActionResult<List<RfmDto>>> GetRFMByIds([FromQuery] string ids)
        {
            var rfms = await _RFMService.GetRFMByIdsAsync(ids);
            return Ok(rfms);
        }

        [ProducesResponseType(typeof(RfmDto), 200)]
        [HttpPost]
        public async Task<IActionResult> Create(BaseRfmDto model)
        {
            return Ok(await _RFMService.CreateAsync(model));
        }

        [ProducesResponseType(typeof(PagedListDTO<RfmDto>), 200)]
        [HttpGet]
        public async Task<IActionResult> GetAll(int? offset, int? limit,
            int? amountFrom = null, int? amountTo = null,
            int? countFrom = null, int? countTo = null,
            int? periodFrom = null, int? periodTo = null)
        {
            var amount = GetRangeModel(amountFrom, amountTo);
            var count = GetRangeModel(countFrom, countTo);
            var period = GetRangeModel(periodFrom, periodTo);

            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(RfmDto), "Id", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }

            return Ok(await _RFMService.GetAllAsync(validatePagingParamsDTO.PagingDTO, amount, count, period));
        }

        [ProducesResponseType(typeof(NoContentResult), 204)]
        [HttpDelete]
        public async Task Delete(int id)
        {
            await _RFMService.DeleteAsync(id);
        }

        private static RangeDTO<int>? GetRangeModel(int? from, int? to)
        {
            if (from.HasValue || to.HasValue)
            {
                return new RangeDTO<int>
                {
                    From = from ?? 0,
                    To = to ?? int.MaxValue,
                };
            }

            return null;
        }
    }
}
