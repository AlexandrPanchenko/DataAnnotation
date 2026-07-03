using Microsoft.AspNetCore.Mvc;
using JetFlight.Service.Validators;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Admins;
using JetFlight.Service.Services;
using JetFlight.WebApi.Controllers;
using JetFlight.Shared.Constants;
using JetFlight.Shared.UserContext;
using Microsoft.AspNetCore.Authorization;
using JetFlight.WebApi.Authorization;

namespace JetFlight.WebApi.AdminControllers
{

    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class AdminsController : BaseController
    {
        private readonly IAdminService _adminService;
        private readonly IUserContext _userContext;

        public AdminsController(IAdminService adminService, IUserContext userContext)
        {
            _adminService = adminService;
            _userContext = userContext;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(GetFullAdminDTO), 200)]
        public async Task<IActionResult> Me()
        {
            var adminDTO = await _adminService.GetAllById(_userContext.AdminId.Value);
            return Ok(adminDTO);
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<GetFullAdminDTO>), 200)]
        public async Task<IActionResult> GetAllByPermissions(AdminFilterDTO adminFilterDTO)
        {
            var admins = await _adminService.GetAllByPermissions(adminFilterDTO.pagePermissionType, adminFilterDTO.crud, adminFilterDTO.searchParam);
            return Ok(admins);
        }

        [HttpPost("[action]/email")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticateResponse), 200)]
        public async Task<IActionResult> Authenticate(AuthenticateEmailRequest model)
        {
            var response = await _adminService.Authenticate(model);
            if (response == null)
            {
                return BadRequest("Не правильний логін або пароль");
            }
            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GetAdminDTO), 200)]
        public async Task<IActionResult> GetAdminById(int id)
        {
            var admins = await _adminService.GetById(id);
            return Ok(admins);
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedListDTO<GetAdminDTO>), 200)]
        public async Task<IActionResult> GetAdmins(string? searchParam = null, int? offset = null, int? limit = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(typeof(GetAdminDTO),
              "CreatedAt",
               OrderByDirectionTypes.DESC.ToString(),
              offset,
              limit,
              int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }
            var admins = await _adminService.GetAdmins(validatePagingParamsDTO.PagingDTO, searchParam);
            return Ok(admins);
        }

        [HttpPost("create")]
        [HasPermission(Permission.Users, PermissionLevel.Modify)]
        public async Task<IActionResult> Create(AdminCreateDTO admin)
        {
            var newAdmin = await _adminService.CreateAdmin(admin);
            if (newAdmin.Errors.Any())
            {
                return BadRequest(CreateErrorResponseModel(newAdmin.Errors));
            }
            return Ok(newAdmin);
        }

        [HttpPost("update")]
        [HasPermission(Permission.Users, PermissionLevel.Modify)]
        public async Task<IActionResult> Update(AdminUpdateDTO admin)
        {
            var updatedAdmin = await _adminService.UpdateAdmin(admin);
            if (updatedAdmin.Errors.Any())
            {
                return BadRequest(CreateErrorResponseModel(updatedAdmin.Errors));
            }
            return Ok(updatedAdmin);
        }

        [HttpPost("validatePasswordSetupOTP")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidatePasswordSetup(ValidateResetPasswordDTO validateResetPasswordDTO)
        {
            var result = await _adminService.ValidatePasswordSetup(validateResetPasswordDTO);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpPost("setupAdminPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> SetupAdminPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var result = await _adminService.SetupAdminPassword(resetPasswordDTO);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("CreatePasswordResetLink")]
        [ProducesResponseType(typeof(TokenDTO), 200)]
        public async Task<IActionResult> CreatePasswordResetLink(TokenDTO token)
        {
            var result = await _adminService.CreatePasswordResetLink(token.AuthCode);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("RequestPasswordReset")]
        [ProducesResponseType(typeof(RequestResetPasswordDTO), 200)]
        public async Task<IActionResult> RequestPasswordReset(RequestResetPasswordDTO requestResetPassword)
        {
            var result = await _adminService.RequestResetPassword(requestResetPassword.Email);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
    }
}