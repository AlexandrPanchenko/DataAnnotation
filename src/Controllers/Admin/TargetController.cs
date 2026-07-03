using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.WebApi.Controllers;
using JetFlight.Shared.Models.Shared;
using JetFlight.Service.Validators;
using JetFlight.Shared.UserContext;
using JetFlight.Shared.Models.Targets;
using JetFlight.Shared.Models.Store;
using JetFlight.WebApi.Authorization;

namespace JetFlight.WebApi.AdminControllers
{

    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class TargetController : BaseController
    {
        private readonly ITargetService _targetService;
        public TargetController(ITargetService targetService)
        {
            _targetService = targetService;
        }


        [ProducesResponseType(typeof(TargetDto), 200)]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _targetService.GetAsync(id));
        }

        [ProducesResponseType(typeof(NoContentResult), 204)]
        [HasPermission(Permission.Target, PermissionLevel.Modify)]
        [HttpPut]
        public async Task<IActionResult> Update(TargetDto model)
        {
            await _targetService.UpdateAsync(model);
            return NoContent();
        }

        [ProducesResponseType(typeof(TargetDto), 200)]
        [HasPermission(Permission.Target, PermissionLevel.Modify)]
        [HttpPost]
        public async Task<IActionResult> Create(BaseTargetDto model)
        {
            return Ok(await _targetService.CreateAsync(model));
        }

        [ProducesResponseType(typeof(PagedListDTO<TargetDto>), 200)]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll(int? offset, int? limit, byte? branchId)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(TargetDto), "Id", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }

            return Ok(await _targetService.GetAllAsync(validatePagingParamsDTO.PagingDTO, branchId));
        }

        [ProducesResponseType(typeof(NoContentResult), 204)]
        [HasPermission(Permission.Target, PermissionLevel.Delete)]
        [HttpDelete]
        public async Task Delete(int id)
        {
            await _targetService.DeleteAsync(id);
        }

        [ProducesResponseType(typeof(Dictionary<Branches, int>), 200)]
        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> CalculateAudience(int targetId)
        {
            return Ok(await _targetService.CalculateAudienceAsync(targetId));
        }
    }
}
