using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.ContactUs;
using JetFlight.WebApi.Authorization;
using Microsoft.AspNetCore.Mvc;
using JetFlight.Shared.Extensions;
using JetFlight.Shared.Models.Store;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace JetFlight.WebApi.Controllers
{
  [ApiController]
  [ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
  [Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
  public class ContactUsController : BaseController
  {
    private readonly IContactUsService _contactUsService;
    public ContactUsController(IContactUsService contactUsService)
    {
      _contactUsService = contactUsService;
    }

    [HttpPost("create")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetContactUsResponseDTO), 200)]
    public async Task<IActionResult> Create([FromForm] ContactUsDTO contactUsDto, [FromHeader][Required] Branches branchId)
    {
      // Якщо форма відправлена з сайту CatJet — лист має йти від CatJet (info@catjet.online), а не від BirdJet
      var referer = Request.Headers.Referer.FirstOrDefault();
      var effectiveBranchId = !string.IsNullOrEmpty(referer) && referer.Contains("catjet", StringComparison.OrdinalIgnoreCase)
        ? Branches.CatJet
        : branchId;
      var createdSeo = await _contactUsService.CreateContactUs(contactUsDto, (byte)effectiveBranchId);
      return Ok(createdSeo);
    }

    [HttpGet("[action]")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<TopicDTO>), 200)]
    public async Task<IActionResult> GetTopics()
    {
      var logsHistory = await _contactUsService.GetAllTopics();
      return Ok(logsHistory);
    }

  }
}
