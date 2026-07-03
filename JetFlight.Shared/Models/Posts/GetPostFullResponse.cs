using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Posts
{
    public class GetPostFullResponse : GetPostResponse
    {
        [Required]
        public List<GetPostTagResponse> PostTags { get; set; } = default!;
    }
}
