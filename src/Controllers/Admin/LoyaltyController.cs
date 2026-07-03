using System;
using System.Threading.Tasks;
using JetFlight.Service.Services;
using JetFlight.Service.Validators;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Promotion;
using JetFlight.Shared.Models.Shared;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Models.Users;
using JetFlight.Shared.UserContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JetFlight.WebApi.Controllers.Admin;

[Authorize(Roles = UserRole.Admin)]
[ApiController]
[ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
[Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
public class LoyaltyController : BaseController
{
    private readonly IFlightLoyaltyService _flightLoyaltyService;

    public LoyaltyController(IFlightLoyaltyService flightLoyaltyService)
    {
        _flightLoyaltyService = flightLoyaltyService;
    }

    [ProducesResponseType(typeof(PagedListDTO<PromotionDetailsAdminDTO>), 200)]
    [HttpGet]
    public async Task<ActionResult<PagedListDTO<PromotionDetailsAdminDTO>>> GetAllPromotions(
       [FromQuery] PromotionFilterDTO filterDto)
    {
        var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(typeof(PromotionDetailsAdminDTO),
            null,
            null,
            filterDto.offset,
            filterDto.limit,
            int.MaxValue);

        if ((validatePagingParamsDTO.Errors.Any()))
        {
            return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
        }

        var promotions = await _flightLoyaltyService.GetAllPromotionsAdmin(
            validatePagingParamsDTO.PagingDTO,
            filterDto.searchParam,
            filterDto.startedDate,
            filterDto.cities,
            filterDto.promotionTagIds,
            filterDto.categoryCode,
            filterDto.promotionTypeNavisionId);

        return Ok(promotions);
    }

    [ProducesResponseType(typeof(List<PromotionsTypeDTO>), 200)]
    [HttpGet("types")]
    public async Task<ActionResult<List<PromotionsTypeDTO>>> GetAllPromotionTypes()
    {
        var promotionTypes = await _flightLoyaltyService.GetAllPromotionTypes(true);
        return Ok(promotionTypes);
    }

    [ProducesResponseType(typeof(List<PromotionsCategoryDTO>), 200)]
    [HttpGet("categories")]
    public async Task<ActionResult<List<PromotionsCategoryDTO>>> GetAllPromotionCategories()
    {
        var promotionCategories = await _flightLoyaltyService.GetAllPromotionCategories(true);
        return Ok(promotionCategories);
    }

    [HttpGet("tags")]
    [ProducesResponseType(typeof(List<PromotionsTagDTO>), 200)]
    public async Task<ActionResult<List<PromotionsTagDTO>>> GetAllPromotionTags()
    {
        var promotionTags = await _flightLoyaltyService.GetAllPromotionTags(true);
        return Ok(promotionTags);
    }

    [HttpPut("[action]")]
    [ProducesResponseType(typeof(NoContentResult), 204)]
    public async Task<IActionResult> UpdatePromotionType(int id, [FromBody] UpdatePromotionTypeDTO updateDto)
    {
        var result = await _flightLoyaltyService.UpdatePromotionType(id, updateDto);

        if (!result)
        {
            return NotFound("Loyalty offer type not found.");
        }

        return Ok("Loyalty offer type updated.");
    }

    [HttpPut("[action]")]
    [ProducesResponseType(typeof(NoContentResult), 204)]
    public async Task<IActionResult> UpdatePromotionCategory(string categoryCode, [FromForm] UpdatePromotionCategoryDTO updateDto)
    {
        var result = await _flightLoyaltyService.UpdatePromotionCategory(categoryCode, updateDto);

        if (!result)
        {
            return NotFound("Category not found.");
        }

        return Ok("Category updated.");
    }

    [HttpPut("[action]")]
    [ProducesResponseType(typeof(NoContentResult), 204)]
    public async Task<IActionResult> UpdatePromotionTag(int id, [FromForm] UpdatePromotionTagDTO updateDto)
    {
        var result = await _flightLoyaltyService.UpdatePromotionTag(id, updateDto);

        if (!result)
        {
            return NotFound("Tag not found.");
        }

        return Ok("Tag updated.");
    }

    [ProducesResponseType(typeof(NoContentResult), 204)]
    [HttpPut]
    public async Task<IActionResult> UpdatePromotion(int promotionId, [FromForm] UpdatePromotionDTO updatePromotionDto)
    {
        if (updatePromotionDto == null)
        {
            return BadRequest("Invalid request.");
        }

        var result = await _flightLoyaltyService.UpdatePromotion(promotionId, updatePromotionDto);

        if (!result)
        {
            return NotFound("Loyalty offer not found.");
        }

        return Ok("Loyalty offer updated.");
    }

    [HttpGet("displayRules")]
    [ProducesResponseType(typeof(List<PromotionDisplayRuleDTO>), 200)]
    public async Task<IActionResult> GetPromotionDisplayRules()
    {
        return Ok(await _flightLoyaltyService.GetDisplayRulesAsync());
    }

    [HttpGet("displayRules/{branchId}")]
    [ProducesResponseType(typeof(PromotionDisplayRuleDTO), 200)]
    public async Task<IActionResult> GetPromotionDisplayRule(Branches branchId)
    {
        return Ok(await _flightLoyaltyService.GetDisplayRuleAsync(branchId));
    }

    [HttpPut("displayRules")]
    [ProducesResponseType(typeof(NoContentResult), 204)]
    public async Task<IActionResult> UpdatePromotionDisplayRule(PromotionDisplayRuleDTO model)
    {
        await _flightLoyaltyService.UpdateDisplayRuleAsync(model);
        return NoContent();
    }

    [HttpPost("[action]")]
    [ProducesResponseType(typeof(SavedPromotionNotificationResultDTO), 200)]
    public async Task<IActionResult> TriggerSavedPromotionExpirationNotification([FromQuery] int? promotionId = null)
    {
        if (promotionId.HasValue && promotionId.Value <= 0)
        {
            return BadRequest(new { message = "PromotionId must be greater than 0 if provided" });
        }

        try
        {
            var result = await _flightLoyaltyService.TriggerSavedPromotionExpirationNotificationManuallyAsync(promotionId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}
