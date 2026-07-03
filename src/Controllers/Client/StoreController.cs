using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Store;
using JetFlight.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace JetFlight.WebApi.Controllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
    [Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
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
        public async Task<IActionResult> GetAllStores([FromHeader][Required] Branches branchId, int? cityId = null, string? searchField = null)
        {
            var stores = await _storeService.GetAll(cityId, searchField, (int)branchId);
            return Ok(stores);
        }

        [HttpGet("[action]")]
        [ProducesResponseType(typeof(List<StoreResponseDTO>), 200)]
        [SetCulture("uk-UA")]
        public async Task<IActionResult> GetStoresByIds([FromHeader][Required] Branches branchId, string ids)
        {
            var stores = await _storeService.GetByIds(ids, (byte) branchId);
            return Ok(stores);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(StoreResponseDTO), 200)]
        [SetCulture("uk-UA")]
        public async Task<IActionResult> GetStoreById(int id)
        {
            return Ok(await _storeService.GetStore(id));
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<CityResponseDTO>), 200)]
        public async Task<IActionResult> GetCities([FromHeader][Required] Branches branchId)
        {
            var branch = await _storeService.GetCities((byte)branchId);
            return Ok(branch);
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<StoreResponseDTO>), 200)]
        [SetCulture("uk-UA")]
        public async Task<IActionResult> GetClosestStores(
            [FromHeader][Required] Branches branchId,
            [Required][RegularExpression(RegexConstants.Latitude)] string latitude,
            [Required][RegularExpression(RegexConstants.Longitude)] string longitude, int limit)
        {
            return Ok(await _storeService.GetClosestStores((byte)branchId, latitude, longitude, limit));
        }
    }
}
