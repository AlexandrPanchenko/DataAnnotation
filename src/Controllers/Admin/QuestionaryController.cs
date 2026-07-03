using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.WebApi.Controllers;
using JetFlight.Shared.Models.Shared;
using JetFlight.Service.Validators;
using JetFlight.Shared.UserContext;
using JetFlight.Shared.Models.Questionary;
using JetFlight.Shared.Models.Export;
using JetFlight.Shared.Models;

namespace JetFlight.WebApi.AdminControllers
{

    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class QuestionaryController : BaseController
    {
        private readonly IQuestionaryService _questionaryService;
        public QuestionaryController(IQuestionaryService questionaryService)
        {
            _questionaryService = questionaryService;
        }


        [ProducesResponseType(typeof(AdminQuestionaryDTO), 200)]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _questionaryService.GetAsync(id));
        }

        [ProducesResponseType(typeof(NoContentResult), 204)]
        [HttpPut]
        public async Task<IActionResult> Update(UpdateQuestionaryDTO model)
        {
            await _questionaryService.UpdateAsync(model);
            return NoContent();
        }

        [ProducesResponseType(typeof(AdminQuestionaryDTO), 200)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateQuestionaryDTO model)
        {
            return Ok(await _questionaryService.CreateAsync(model));
        }

        [ProducesResponseType(typeof(PagedListDTO<AdminQuestionaryDTO>), 200)]
        [HttpGet]
        public async Task<IActionResult> GetAll(int? offset, int? limit,
            byte? branchId,
            DateOnly? date = null,
            QuestionaryStatus? status = null
            )
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(AdminQuestionaryDTO), "Id", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }

            return Ok(await _questionaryService.GetAllAsync(validatePagingParamsDTO.PagingDTO, branchId, date, status));
        }

        [ProducesResponseType(typeof(NoContentResult), 204)]
        [HttpDelete]
        public async Task Delete(int id)
        {
            await _questionaryService.DeleteAsync(id);
        }

        [ProducesResponseType(typeof(PagedListDTO<CustomerQuestionaryAnswerDTO>), 200)]
        [HttpGet("{questionaryId}/answers")]
        public async Task<IActionResult> GetAnswers(int questionaryId, int? offset = null, int? limit = null, byte? branchId = null, ClientPlatform? clientPlatform = null, DateOnly? date = null)
        {
            var validatePagingParamsDTO = PagingValidator.ValidatePagingParams(
                typeof(CustomerQuestionaryAnswerDTO), "Id", OrderByDirectionTypes.DESC.ToString(), offset, limit, int.MaxValue);

            if ((validatePagingParamsDTO.Errors.Any()))
            {
                return BadRequest(CreateErrorResponseModel(validatePagingParamsDTO.Errors));
            }

            return Ok(await _questionaryService.GetAnswersAsync(validatePagingParamsDTO.PagingDTO, questionaryId, branchId, clientPlatform, date));
        }

        [ProducesResponseType(typeof(NoContentResult), 204)]
        [HttpPost("archive")]
        public async Task Archive(int id)
        {
            await _questionaryService.ArchiveAsync(id);
        }

        [ProducesResponseType(typeof(NoContentResult), 204)]
        [HttpPost("deactivate")]
        public async Task Deactivate(int id)
        {
            await _questionaryService.DeactivateAsync(id);
        }

        [ProducesResponseType(typeof(NoContentResult), 204)]
        [HttpPost("publish")]
        public async Task Publish(int id)
        {
            await _questionaryService.PublishAsync(id);
        }

        [ProducesResponseType(typeof(FileResult), 200)]
        [HttpPost("Export")]
        public async Task<IActionResult> Export(int id, ExportFileFormat format, byte? branchId = null, ClientPlatform? clientPlatform = null, DateOnly? date = null)
        {
            var exportFile = await _questionaryService.ExportAsync(id, format, branchId, clientPlatform);
            return File(exportFile.Stream, exportFile.ContentType, exportFile.FileName);
        }
    }
}
