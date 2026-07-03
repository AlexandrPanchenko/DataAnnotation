using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;
using JetFlight.Shared.Models.Loyalty;
using JetFlight.Service.Services;
using JetFlight.Shared.Exceptions;


namespace JetFlight.WebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = UserRole.Cashdesk)]
    [ApiExplorerSettings(GroupName = RouteConstants.Cashdesk.BasePathName)]
    [Route($"v1/{RouteConstants.Cashdesk.BasePathName}/[controller]")]
    [Produces("application/xml")]
    [Consumes("application/xml")]
    public class CustomerLoyaltyController : BaseController
    {
        private readonly ILoyaltyService _loyaltyService;
        private readonly ICustomerService _customerService;
        public CustomerLoyaltyController(ILoyaltyService loyaltyService, ICustomerService customerService)
        {
            _loyaltyService = loyaltyService;
            _customerService = customerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(LoyaltyDto), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> GetLoyalty(string cardCode)
        {
            try
            {
                var loyalty = await _loyaltyService.GetLoyaltyAsync(cardCode);
                return Ok(loyalty);
            }
            catch (NotFoundException ex)
            {
                var error = new ErrorResponse { Error = ex.Message };
                return NotFound(error);
            }
            catch (BadRequestException ex)
            {
                var error = new ErrorResponse { Error = ex.Message };
                return BadRequest(error);
            }
        }

        public class ErrorResponse
        {
            public string Error { get; set; }
        }

        [HttpPost("UseBonuses")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> UseBonuses(UseBonusesDto model)
        {
            try
            {
                var groupId = await _customerService.UseCustomerBonusesAsync(model);
                return Ok(groupId);
            }
            catch (NotFoundException ex)
            {
                var error = new ErrorResponse { Error = ex.Message };
                return NotFound(error);
            }
            catch (BadRequestException ex)
            {
                var error = new ErrorResponse { Error = ex.Message };
                return BadRequest(error);
            }
        }
    }
}
