using JetFlight.Shared.Models.PageManagement;
using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Posts
{
    public class GetPageResponse
    {
        [Required]
        public int Total { get; set; }
        public string Title { get; set; }
        public List<PageDTO>? Page { get; set; } = default!;
    }
}
