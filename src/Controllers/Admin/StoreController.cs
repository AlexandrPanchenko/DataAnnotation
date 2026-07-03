using JetFlight.Service.Services;
using JetFlight.Shared.Models.Store;
using JetFlight.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;
using JetFlight.Shared.Constants;
using JetFlight.WebApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;
using System.ComponentModel.DataAnnotations;

namespace JetFlight.WebApi.AdminControllers
{
    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class StoreController : BaseController
    {
        private readonly IStoreService _storeService;
        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<StoreResponseDTO>), 200)]
        [SetCulture("uk-UA")]
        public async Task<IActionResult> GetAllStores(int? cityId = null, string? searchField = null, int? branchId = null)
        {
            var stores = await _storeService.GetAll(cityId, searchField, branchId);
            return Ok(stores);
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<StoreResponseDTO>), 200)]
        [SetCulture("uk-UA")]
        public async Task<IActionResult> GetStoresByIds(string ids)
        {
            var stores = await _storeService.GetByIds(ids);
            return Ok(stores);
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<CityResponseDTO>), 200)]
        public async Task<IActionResult> GetCities([FromQuery] Branches? branchId = null)
        {
            var cities = await _storeService.GetCities((byte?)branchId);
            return Ok(cities);
        }

        [HttpPost("update")]
        [HasPermission(Permission.Stores, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(StoreUpdateResponse), 200)]
        [SetCulture("uk-UA")]
        public async Task<IActionResult> Update([FromForm] StoreUpdateRequest store)
        {
            var updatedStore = await _storeService.UpdateStore(store);
            if (updatedStore.Errors.Any())
            {
                return BadRequest(CreateErrorResponseModel(updatedStore.Errors));
            }
            return Ok(updatedStore);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(StoreResponseDTO), 200)]
        [SetCulture("uk-UA")]
        public async Task<IActionResult> GetStoreById(int id)
        {
            return Ok(await _storeService.GetStore(id));
        }
    }
}
