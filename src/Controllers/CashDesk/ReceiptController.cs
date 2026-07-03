using JetFlight.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using JetFlight.Shared.UserContext;
using JetFlight.Shared.Models.Receipts;
using JetFlight.Service.Services;
using System.Linq;

namespace JetFlight.WebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = UserRole.Cashdesk)]
    [ApiExplorerSettings(GroupName = RouteConstants.Cashdesk.BasePathName)]
    [Route($"v1/{RouteConstants.Cashdesk.BasePathName}/[controller]")]
    [Produces("application/xml")]
    [Consumes("application/xml")]
    public class ReceiptController : BaseController
    {
        private readonly IReceiptService _receiptService;
        public ReceiptController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> Create(CreateReceiptDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new { message = "Model validation failed", errors });
            }
            
            var id = await _receiptService.CreateAsync(model);
            return Ok(id);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> Delete(int id)
        {
            await _receiptService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("Deactivate")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> Deactivate(int receiptId)
        {
            var result = await _receiptService.DeactivateAsync(receiptId);
            return Ok(result);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(NoContentResult), 204)]
        public async Task<IActionResult> Delete(string cardCode, string transactionNumber, string posTerminal, string storeCode, string receiptNumber)
        {
            await _receiptService.DeleteAsync(cardCode, transactionNumber, posTerminal, storeCode, receiptNumber);
            return NoContent();
        }
    }
}
