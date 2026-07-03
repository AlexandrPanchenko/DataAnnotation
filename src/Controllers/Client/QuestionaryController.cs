using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.Models.Questionary;
using JetFlight.Shared.Models;
using JetFlight.Shared.Models.Store;
using System.ComponentModel.DataAnnotations;

namespace JetFlight.WebApi.Controllers.Client
{

    [Authorize]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Client.BasePathName)]
    [Route($"v1/{RouteConstants.Client.BasePathName}/[controller]")]
    public class QuestionaryController : BaseController
    {
        private readonly IQuestionaryService _questionaryService;
        public QuestionaryController(IQuestionaryService questionaryService)
        {
            _questionaryService = questionaryService;
        }


        [ProducesResponseType(typeof(CustomerQuestionaryDTO), 200)]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _questionaryService.GetCustomerQuestionaryByIdAsync(id));
        }

        [ProducesResponseType(typeof(List<CustomerQuestionaryDTO>), 200)]
        [HttpGet]
        public async Task<IActionResult> GetQuestionaries()
        {
            return Ok(await _questionaryService.GetCustomerQuestionariesAsync());
        }

        [ProducesResponseType(typeof(QuestionaryAnswerResponse), 200)]
        [HttpPost("answer")]
        public async Task<IActionResult> Answer([FromHeader][Required] ClientPlatform platform, [FromHeader][Required] Branches branchId, QuestionaryAnswerDTO model)
        {
            // branchId визначає, від якої гілки йде лист (1 = BirdJet, 2 = CatJet). Клієнт передає заголовок залежно від сайту.
            var response = await _questionaryService.AnswerAsync(model, platform, (byte)branchId);
            return Ok(response);
        }
    }
}
