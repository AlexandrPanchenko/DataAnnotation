using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using JetFlight.Shared.Models.Store;
using System.ComponentModel.DataAnnotations;
using JetFlight.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Users;
using JetFlight.Shared.UserContext;
using JetFlight.Shared.Models.Avatars;
using JetFlight.Service.Validators;

namespace JetFlight.WebApi.Controllers
{
    [Authorize(Roles = UserRole.Customer)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
    [Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
    public class CustomerController : BaseController
    {
        private readonly ICustomerService _customerService;
        private readonly IAvatarService _avatarService;
        private readonly IUserContext _userContext;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IJwtUtils _jwtUtils;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerController(
            ICustomerService customerService,
            IAvatarService avatarService,
            IUserContext userContext,
            IRefreshTokenService refreshTokenService,
            IJwtUtils jwtUtils,
            IHttpContextAccessor httpContextAccessor)
        {
            _customerService = customerService;
            _avatarService = avatarService;
            _userContext = userContext;
            _refreshTokenService = refreshTokenService;
            _jwtUtils = jwtUtils;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CustomerDTO), 200)]
        public async Task<IActionResult> Get()
        {
            return Ok(await _customerService.Get());
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SendPhoneNumberVerificationCode([FromBody] string newPhoneNumber)
        {
            try
            {
                await _customerService.SendPhoneNumberVerificationCode(newPhoneNumber);
                return Ok(new { message = "Відправлено код веріфікації" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(VerifyAndUpdatePhoneNumberResponse), 200)]
        public async Task<IActionResult> VerifyAndUpdatePhoneNumber([FromBody] VerifyPhoneNumberRequest model)
        {
            try
            {
                var result = await _customerService.VerifyAndUpdatePhoneNumber(model.NewPhoneNumber, model.VerificationCode);
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(new { message = result.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> Update(CustomerUpdateDTO model)
        {
            await _customerService.Update(model);
            return new NoContentResult();
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> AddCard(AddCustomerCardDTO model)
        {
            var result = await _customerService.AddCard(model);

            if (result.Errors.Any())
            {
                return BadRequest(CreateErrorResponseModel(result.Errors));
            }

            return new NoContentResult();
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> SendCode([FromHeader][Required] RegistrationPlatform platform, [FromHeader][Required] Branches branchId, CustomerSendAuthenticateCodeRequest model, [FromQuery] string? token = null)
        {
            try
            {
                await _customerService.SendCode(platform, model, branchId, token);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticateResponse), 200)]
        public async Task<IActionResult> Authenticate([FromHeader][Required] RegistrationPlatform platform,
            [FromHeader][Required] Branches branchId,
             CustomerAuthenticateRequest model)
        {
            var response = await _customerService.Authenticate(model, branchId, platform);
            if (response == null)
            {
                return BadRequest("Код не знайденний або не валідний для цього номера.");
            }
            return Ok(response);
        }

        /// <summary>
        /// Refresh access token using refresh token (token rotation)
        /// </summary>
        [HttpPost("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticateResponse), 200)]
        public async Task<IActionResult> Refresh(RefreshTokenRequest model)
        {
            var (ipAddress, userAgent) = GetClientInfo();

            var result = await _refreshTokenService.RotateRefreshTokenAsync(
                model.RefreshToken,
                ipAddress,
                userAgent
            );

            if (result == null)
            {
                return BadRequest("Invalid or expired refresh token");
            }

            var (newRefreshToken, refreshTokenEntity, customer) = result.Value;
            var branchId = (Branches)refreshTokenEntity.BranchId;

            // Generate new JWT access token
            var jwtToken = _jwtUtils.GenerateJwtToken(customer, branchId);

            return Ok(new AuthenticateResponse(jwtToken, newRefreshToken));
        }

        /// <summary>
        /// Sign out - revokes the refresh token
        /// </summary>
        [HttpPost("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> SignOut(SignOutRequest model)
        {
            // Attempt to revoke the refresh token
            // Always return NoContent regardless of success - this is idempotent
            // If token is already revoked or doesn't exist, sign out is already achieved
            await _refreshTokenService.RevokeRefreshTokenAsync(model.RefreshToken, "User logout");

            return NoContent();
        }

        /// <summary>
        /// Extracts client IP address and user agent from HTTP context
        /// </summary>
        private (string? ipAddress, string? userAgent) GetClientInfo()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return (null, null);
            }
            
            // Get IP address (supports X-Forwarded-For for proxies/load balancers)
            var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            }

            // Get user agent
            var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault();

            return (ipAddress, userAgent);
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<AvatarDTO>), 200)]
        public IActionResult GetAvatars([FromHeader][Required] Branches branchId)
        {
            return Ok(_avatarService.GetAvatars(branchId));
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            var customerId = _userContext.CustomerId;
            var result = await _customerService.MarkAsReadAsync(notificationId, customerId);

            if (!result)
            {
                return NotFound("Повідомлення не знайдено");
            }

            return Ok("Повідомленя помічене як прочитане");
        }

        [HttpGet("verify-email")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(VerifyEmailResponse), 200)]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token, [FromHeader] byte? branchId = null, [FromQuery(Name = "branchId")] byte? branchIdFromQuery = null)
        {
            try
            {
                var effectiveBranchId = branchId ?? branchIdFromQuery;
                var result = await _customerService.VerifyEmail(token, effectiveBranchId);
                // Якщо в респонсі є redirectUrl — редіректимо незалежно від Success
                if (!string.IsNullOrEmpty(result.RedirectUrl))
                {
                    return Redirect(result.RedirectUrl);
                }

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("[action]")]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var customerId = _userContext.CustomerId;
            await _customerService.MarkAllAsReadAsync(customerId);

            return Ok("Всі повідомленя помічені як прочитані.");
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            var customerId = _userContext.CustomerId;
            await _customerService.DeleteAllNotificationsAsync(customerId);

            return Ok("Всі повідомлення видалені");
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(NotificationHistoryResponseDTO), 200)]
        public async Task<IActionResult> GetNotificationHistory(
        [FromQuery] string? orderBy = null,
        [FromQuery] string? orderByDirection = null,
        [FromQuery] int? offset = null,
        [FromQuery] int? limit = null)
        {
            try
            {
                var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(typeof(GetAllCustomersResponse),
                    orderBy,
                    orderByDirection,
                    offset,
                    limit,
                    int.MaxValue);
                var customerId = _userContext.CustomerId;

                if(orderBy == null)
                {
                    validatePagingParamsDTO.PagingDTO.OrderBy = "CreatedAt"; 
                    validatePagingParamsDTO.PagingDTO.OrderByDirectionType = OrderByDirectionTypes.DESC;
                }
                if ((validatePagingParamsDTO.Errors.Any()))
                {
                    return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
                }
                var (notifications, totalRecords, unreadRecords) = await _customerService.GetNotificationHistoryAsync(validatePagingParamsDTO.PagingDTO, customerId);

                var response = new NotificationHistoryResponseDTO
                {
                    Notifications = notifications,
                    TotalRecords = totalRecords,
                    UnreadRecords = unreadRecords
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(PagedListDTO<ReceiptDTO>), 200)]
        public async Task<IActionResult> GetPurchaseHistory(
        [FromHeader][Required] Branches branchId,
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
                var purchaseHistory = await _customerService.GetPurchaseHistory(validatePagingParamsDTO.PagingDTO, null, productName, startDate, endDate, (byte)branchId);
                return Ok(purchaseHistory);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> Delete()
        {
            await _customerService.DeleteCustomerByIdAsync(_userContext.CustomerId!.Value);
            return NoContent();

        }
    }
}
