using JetFlight.Service.Services;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.LogHistory;
using JetFlight.Shared.UserContext;
using JetFlight.WebApi.Authorization;
using JetFlight.WebApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JetFlight.WebApi.AdminControllers
{

    [Authorize(Roles = UserRole.Admin)]
    [AllowAnonymous]
    [ApiController]
    [ApiExplorerSettings(GroupName = RouteConstants.Admin.BasePathName)]
    [Route($"v1/{RouteConstants.Admin.BasePathName}/[controller]")]
    public class LogHistoryController : BaseController
    {
        private readonly ILogHistoryService _logHistoryService;
        public LogHistoryController(ILogHistoryService logHistoryService)
        {
            _logHistoryService = logHistoryService;
        }

        [HttpGet("byAdminId/{adminId}")]
        [ProducesResponseType(typeof(LogHistoryDTO), 200)]
        public async Task<IActionResult> GetLogsHistory(int adminId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var logsHistory = await _logHistoryService.GetAll(adminId, dateFrom, dateTo);
            return Ok(logsHistory);
        }

        [HttpGet("byPostId/{postId}")]
        [ProducesResponseType(typeof(List<LogHistoryDTO>), 200)]
        public async Task<IActionResult> GetLogsHistoryByPost(int postId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var logsHistory = await _logHistoryService.GetByPost(postId, dateFrom, dateTo);
            return Ok(logsHistory);
        }

        [HttpGet("byStoreId/{storeId}")]
        [ProducesResponseType(typeof(List<LogHistoryDTO>), 200)]
        public async Task<IActionResult> GetLogsHistoryByStore(int storeId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var logsHistory = await _logHistoryService.GetByStore(storeId, dateFrom, dateTo);
            return Ok(logsHistory);
        }

        [HttpGet("byPageId/{pageId}")]
        [ProducesResponseType(typeof(List<LogHistoryDTO>), 200)]
        public async Task<IActionResult> GetPagesLogsHistoryByPageId(int pageId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var logsHistory = await _logHistoryService.GetPageByPageId(pageId, dateFrom, dateTo);
            return Ok(logsHistory);
        }

        [HttpGet("byRoleId/{roleId}")]
        [ProducesResponseType(typeof(List<LogHistoryDTO>), 200)]
        public async Task<IActionResult> GetRolesLogsHistory(int roleId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var logsHistory = await _logHistoryService.GetPageByRoleId(roleId, dateFrom, dateTo);
            return Ok(logsHistory);
        }

        [HttpGet("byContactUsId/{contactUsId}")]
        [ProducesResponseType(typeof(List<LogHistoryDTO>), 200)]
        public async Task<IActionResult> GetContactUsLogsHistory(int contactUsId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var logsHistory = await _logHistoryService.GetByContactUs(contactUsId, dateFrom, dateTo);
            return Ok(logsHistory);
        }

        [HttpGet("byFeedbackId/{feedbackId}")]
        [ProducesResponseType(typeof(List<LogHistoryDTO>), 200)]
        public async Task<IActionResult> GetFeedbackogsHistory(int feedbackId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var logsHistory = await _logHistoryService.GetByFeedback(feedbackId, dateFrom, dateTo);
            return Ok(logsHistory);
        }

        [HttpGet("byCouponId/{couponId}")]
        [ProducesResponseType(typeof(List<LogHistoryDTO>), 200)]
        public async Task<IActionResult> GetCouponLogsHistory(int couponId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var logsHistory = await _logHistoryService.GetByCoupon(couponId, dateFrom, dateTo);
            return Ok(logsHistory);
        }

        [HttpGet("byAccumulationCardId/{accumulationCardId}")]
        [ProducesResponseType(typeof(List<LogHistoryDTO>), 200)]
        public async Task<IActionResult> GetAccumulationCardLogsHistory(int accumulationCardId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var logsHistory = await _logHistoryService.GetByAccumulationCard(accumulationCardId, dateFrom, dateTo);
            return Ok(logsHistory);
        }
    }
}
