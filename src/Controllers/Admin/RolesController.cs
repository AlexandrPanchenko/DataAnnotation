using Microsoft.AspNetCore.Mvc;
using JetFlight.Service.Services;
using JetFlight.Shared.Models.Roles;
using JetFlight.WebApi.Controllers;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;
using JetFlight.WebApi.Authorization;

namespace JetFlight.WebApi.AdminControllers
{
    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class RolesController : BaseController
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }


        [HttpGet("permissions")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<GetRolePermissionsResponse>), 200)]
        public async Task<IActionResult> GetPermissions()
        {
            return Ok(await _roleService.GetAllRolesPermissions());
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GetRolesResponse), 200)]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(await _roleService.GetRoles());
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GetRoleFullResponse), 200)]
        public async Task<IActionResult> GetRole(int id)
        {
            return Ok(await _roleService.GetRole(id));
        }

        [HttpPost("create")]
        [HasPermission(Permission.Users, PermissionLevel.Modify)]
        public async Task<IActionResult> Create(RoleCreateRequest role)
        {
            var newRole = await _roleService.CreateRole(role);
            if (newRole.Errors.Count != 0)
            {
                return BadRequest(CreateErrorResponseModel(newRole.Errors));
            }
            return Ok(newRole);
        }

        [HttpPost("update")]
        [HasPermission(Permission.Users, PermissionLevel.Modify)]
        public async Task<IActionResult> Update(RoleUpdateRequest role)
        {
            var updatedRole = await _roleService.UpdateRole(role);
            if (updatedRole.Errors.Count != 0)
            {
                return BadRequest(CreateErrorResponseModel(updatedRole.Errors));
            }
            return Ok(updatedRole);
        }

        [HttpDelete("delete")]
        [HasPermission(Permission.Users, PermissionLevel.Delete)]
        public async Task<IActionResult> Delete(int roleId)
        {
            var result = await _roleService.DeleteRole(roleId);
            if (result.Errors.Count != 0)
            {
                return BadRequest(CreateErrorResponseModel(result.Errors));
            }
            return Ok(result);
        }
    }
}