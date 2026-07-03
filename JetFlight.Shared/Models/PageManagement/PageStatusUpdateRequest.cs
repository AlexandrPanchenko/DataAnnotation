using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.PageManagement
{
    public class PageStatusUpdateRequest
    {
        [Required]
        public int PageId { get; set; }
        [Required]
        public bool IsActive { get; set; }
    }
}
