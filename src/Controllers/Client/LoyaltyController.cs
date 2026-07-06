using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Service.Services;
using JetFlight.Service.Validators;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.FlightLoyalty;
using JetFlight.Shared.Models.Promotion;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Models.Users;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace JetFlight.WebApi.Controllers.Client;

[ApiController]
[ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
[Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
public class LoyaltyController : BaseController
{
    private readonly IFlightLoyaltyService _flightLoyaltyService;

    public LoyaltyController(IFlightLoyaltyService flightLoyaltyService)
    {
        _flightLoyaltyService = flightLoyaltyService;
    }

    [ProducesResponseType(typeof(PagedListDTO<PromotionDetailsClientDTO>), 200)]
    [HttpGet]
    public async Task<ActionResult<PagedListDTO<PromotionDetailsClientDTO>>> GetAllPromotions(
       [FromHeader][Required] Branches branchId,
       [FromHeader][Required] RegistrationPlatform platform,
       [FromQuery] string? token = null,
       [FromQuery] int? offset = null,
       [FromQuery] int? limit = null,
       [FromQuery] string? searchParam = null,
       [FromQuery] SortingEnum? sortOption = null,
       [FromQuery] DateOnly? createdDate = null,
       [FromQuery] int? store = null,
       [FromQuery] string? promotionTagIds = null,
       [FromQuery] string? categoryCode = null,
       [FromQuery] string? promotionTypeNavisionId = null)
    {
        var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(typeof(PromotionDetailsClientDTO),
            null,
            null,
            offset,
            limit,
            int.MaxValue);

        if ((validatePagingParamsDTO.Errors.Any()))
        {
            return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
        }

        var promotions = await _flightLoyaltyService.GetAllPromotionsClient(
            (byte)branchId,
            platform,
            validatePagingParamsDTO.PagingDTO,
            searchParam,
            createdDate,
            store,
            sortOption,
            promotionTagIds,
            categoryCode,
            promotionTypeNavisionId,
            token);

        return Ok(promotions);
    }

    [HttpPost("ticket-discount")]
    [ProducesResponseType(typeof(decimal), 200)]
    public async Task<ActionResult<decimal>> GetTicketDiscount(
        [FromQuery][Required] int loyaltyOfferId,
        [FromBody][Required] Ticket ticket)
    {
        var discount = await _flightLoyaltyService.GetTicketDiscountAsync(loyaltyOfferId, ticket);
        return Ok(discount);
    }

    [HttpDelete("saved-promotions/{promotionId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteSavedPromotionByPromotionId(int promotionId)
    {
        var result = await _flightLoyaltyService.DeleteSavedPromotion(promotionId);

        if (!result)
        {
            return NotFound("Saved loyalty offer was not found or could not be removed.");
        }

        return Ok("Saved loyalty offer removed successfully.");
    }

    [HttpPost("saved-promotions")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddSavedPromotion([FromBody] AddSavedPromotionDTO addSavedPromotionDto)
    {
        if (addSavedPromotionDto == null)
        {
            return BadRequest("Invalid request.");
        }

        var result = await _flightLoyaltyService.AddSavedPromotion(addSavedPromotionDto);

        if (!result)
        {
            return BadRequest("Unable to save loyalty offer.");
        }

        return Ok("Loyalty offer saved successfully.");
    }

    [HttpGet("saved-promotions")]
    [ProducesResponseType(typeof(PagedListDTO<PromotionDetailsClientDTO>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAllSavedPromotions(
       [FromQuery] int? offset = null,
       [FromQuery] int? limit = null,
       [FromQuery] string? searchParam = null,
       [FromQuery] SortingEnum? sortOption = null)
    {
        var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(typeof(SavedPromotionDTO),
            null,
            null,
            offset,
            limit,
            int.MaxValue);

        var savedPromotions = await _flightLoyaltyService.GetAllSavedPromotions(
            validatePagingParamsDTO.PagingDTO,
            searchParam,
            sortOption);

        if (savedPromotions == null)
        {
            return NotFound("No saved loyalty offers found for this member.");
        }

        return Ok(savedPromotions);
    }

    [ProducesResponseType(typeof(List<PromotionsTypeDTO>), 200)]
    [HttpGet("types")]
    public async Task<ActionResult<List<PromotionsTypeDTO>>> GetAllPromotionTypes()
    {
        var promotionTypes = await _flightLoyaltyService.GetAllPromotionTypes();
        return Ok(promotionTypes);
    }

    [HttpGet("tags")]
    [ProducesResponseType(typeof(List<PromotionsTagDTO>), 200)]
    public async Task<ActionResult<List<PromotionsTagDTO>>> GetAllPromotionTags()
    {
        var promotionTags = await _flightLoyaltyService.GetAllPromotionTags();
        return Ok(promotionTags);
    }

    [ProducesResponseType(typeof(List<PromotionsCategoryDTO>), 200)]
    [HttpGet("categories")]
    public async Task<ActionResult<List<PromotionsCategoryDTO>>> GetAllPromotionCategories()
    {
        var promotionCategories = await _flightLoyaltyService.GetAllPromotionCategories();
        return Ok(promotionCategories);
    }
}
