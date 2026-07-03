using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.Models.Users;
using JetFlight.Shared.UserContext;
using JetFlight.WebApi.Controllers;
using JetFlight.Service.Validators;
using JetFlight.Shared.Models.Shared;

namespace JetFlight.WebApi.AdminControllers
{

    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class CustomerController : BaseController
    {
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(PagedListDTO<ReceiptDTO>), 200)]
        public async Task<IActionResult> GetPurchaseHistory(
        [FromQuery] int customerId,
        [FromQuery] string? orderBy = null,
        [FromQuery] string? orderByDirection = null,
        [FromQuery] int? offset = null,
        [FromQuery] int? limit = null,
        [FromQuery] string? productName = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(typeof(GetAllCustomersResponse),
                    orderBy,
                    orderByDirection,
                    offset,
                    limit,
                    int.MaxValue);

                if ((validatePagingParamsDTO.Errors.Any()))
                {
                    return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
                }
                var purchaseHistory = await _customerService.GetPurchaseHistory(validatePagingParamsDTO.PagingDTO, customerId, productName, startDate, endDate);
                return Ok(purchaseHistory);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AdminCustomerDTO), 200)]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var result = await _customerService.GetCustomerByIdAsync(id);
            if (result == null)
            {
                return NotFound(new { message = "Користувача не знайдено" });
            }

            return Ok(result);
        }

        [HttpPatch]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> Update(CustomerUpdateDTO model)
        {
            await _customerService.Update(model, true);
            return new NoContentResult();
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(PagedListDTO<AdminCustomerDTO>), 200)]
        public async Task<IActionResult> GetAllCustomers([FromQuery] GetAllCustomersRequest request)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(GetAllCustomersResponse),
                request.orderBy,
                request.orderByDirection,
                request.offset,
                request.limit,
                int.MaxValue
            );

            if (validatePagingParamsDTO.Errors.Any())
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }

            var result = await _customerService.GetAll(
                validatePagingParamsDTO.PagingDTO,
                request.searchParam,
                request.branchId,
                request.customerStatus,
                request.registrationPlatform,
                request.dateOfRegistration,
                request.city
            );

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var result = await _customerService.DeleteCustomerByIdAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Користувача не знайдено" });
            }

            return Ok(new { message = "Користувач видалений" });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SendTestNotification(
    [FromBody] TestNotificationRequest request)
        {
            try
            {
                await _customerService.SendTestNotificationAsync(
                    request.CustomerId,
                    request.Title,
                    request.Body,
                    request.Type
                );

                return Ok("Test notification sent successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}
