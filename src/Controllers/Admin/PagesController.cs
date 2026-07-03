using JetFlight.Service.Services;
using JetFlight.Shared.Models.PageManagement;
using JetFlight.Shared.Models.Posts;
using Microsoft.AspNetCore.Mvc;
using JetFlight.WebApi.Controllers;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;
using JetFlight.WebApi.Authorization;

namespace JetFlight.WebApi.AdminControllers
{
    [Authorize(Roles = UserRole.Admin)]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class PagesController : BaseController
    {
        private readonly IPageManagementService _pageManagementService;
        public PagesController(IPageManagementService pageManagementService)
        {
            _pageManagementService = pageManagementService;
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<PageDTO>), 200)]
        public async Task<IActionResult> GetRootPages(int? branchId = null)
        {
            var pages = await _pageManagementService.GetRootPages(branchId);
            return Ok(pages);
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PageDTO), 200)]
        public async Task<IActionResult> GetPage(int pageId)
        {
            var pages = await _pageManagementService.GetPage(pageId);
            return Ok(pages);
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PageDTO), 200)]
        public async Task<IActionResult> GetDraftHeaderFooter(int branchId, RootPage page)
        {
            var pages = await _pageManagementService.GetPageByEnum(page, branchId);
            return Ok(pages);
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<GetSectionsResponse>), 200)]
        public async Task<IActionResult> GetDraftHeaderFooterSections(int branchId, RootPage page)
        {
            var siteSettings = await _pageManagementService.GetDraftHeaderFooterSectionsByPageEnum(page, branchId);
            return Ok(siteSettings);
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GetPageResponse), 200)]
        public async Task<IActionResult> GetAllSubPages(int branchId, RootPage page)
        {
            var pages = await _pageManagementService.GetAllSubPages(page, branchId);
            return Ok(pages);
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<GetSectionsResponse>), 200)]
        public async Task<IActionResult> GetSections(int pageId)
        {
            var siteSettings = await _pageManagementService.GetSectionsForPage(pageId, false);
            return Ok(siteSettings);
        }

        [HttpPost("[action]")]
        [HasPermission(Permission.Content, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(SectionUpdateResponseDTO), 200)]
        public async Task<IActionResult> UpdateSectionFields([FromForm] SectionUpdateRequest section)
        {
            var updatedSection = await _pageManagementService.UpdateSection(section);
            if (updatedSection.Errors.Any())
            {
                return BadRequest(CreateErrorResponseModel(updatedSection.Errors));
            }
            return Ok(updatedSection);
        }

        [HttpPost("[action]")]
        [HasPermission(Permission.Content, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(PageStatusUpdateResponseDTO), 200)]
        public async Task<IActionResult> UpdatePageStatus(PageStatusUpdateRequest pageStatus)
        {
            var updatedPage = await _pageManagementService.UpdatePageStatus(pageStatus);
            if (updatedPage.Errors.Any())
            {
                return BadRequest(CreateErrorResponseModel(updatedPage.Errors));
            }
            return Ok();
        }

        [HttpPost("[action]")]
        [HasPermission(Permission.Content, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(PageUpdateResponseDTO), 200)]
        public async Task<IActionResult> UpdatePageDetails(PageUpdateDTO pageStatus)
        {
            var updatedPage = await _pageManagementService.UpdatePageDetails(pageStatus);
            if (updatedPage.Errors.Any())
            {
                return BadRequest(CreateErrorResponseModel(updatedPage.Errors));
            }
            return Ok(updatedPage);
        }

        [HttpDelete("{pageId:int}")]
        [HasPermission(Permission.Content, PermissionLevel.Delete)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeletePage([FromRoute] int pageId)
        {
            await _pageManagementService.DeletePage(pageId);

            return NoContent();
        }

        [HttpPost("[action]")]
        [HasPermission(Permission.Content, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(PageDTO), 200)]
        public async Task<IActionResult> CopyPage([FromBody] PageCopyRequest page)
        {
            var result = await _pageManagementService.CopyPage(page.Id);

            return Ok(result);
        }


        [HttpPost("[action]")]
        [HasPermission(Permission.Content, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(PageStatusUpdateResponseDTO), 200)]
        public async Task<IActionResult> UpdatePagePublishStatus(PagePublishStatusUpdateRequest pageStatus)
        {
            var updatedPage = await _pageManagementService.UpdatePublishPageStatus(pageStatus);
            if (updatedPage.Errors.Any())
            {
                return BadRequest(CreateErrorResponseModel(updatedPage.Errors));
            }
            return Ok(updatedPage);
        }

        [HttpGet("AutoPublish")]
        [AllowAnonymous]
        public async Task<IActionResult> AutoPublish()
        {
            await _pageManagementService.UpdateAllPageStatus();
            return Ok();
        }

        [HttpPost("create")]
        [HasPermission(Permission.Content, PermissionLevel.Modify)]
        [ProducesResponseType(typeof(PageDTO), 200)]
        public async Task<IActionResult> Create([FromBody]SubPageCreateRequest page)
        {
            var newPage = await _pageManagementService.CreatePageWithSections(page);

            return Ok(newPage);
        }

        [HttpPost("upload")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            List<string> info = new List<string>();
            var filePath = Path.Combine(StorageConstants.PhysicalPath, file.FileName);
            info.Add(filePath);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return Ok(new { FileName = file.FileName, Size = file.Length });
        }
    }
}
