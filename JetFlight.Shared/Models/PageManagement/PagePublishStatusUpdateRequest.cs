using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.PageManagement
{
    public class PagePublishStatusUpdateRequest
    {
        [Required]
        public int PageId { get; set; }
        [Required]
        public bool Published { get; set; }
    }
}
