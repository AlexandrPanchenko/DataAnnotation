using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Posts
{
    public class GetPostsResponse
    {
        [Required]
        public int Total { get; set; }
        public List<GetPostFullResponse>? Posts { get; set; } = default!;
    }
}
