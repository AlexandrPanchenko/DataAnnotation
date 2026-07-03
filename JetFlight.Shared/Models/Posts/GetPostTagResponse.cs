using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Posts
{
    public class GetPostTagResponse
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Title { get; set; } = null;
        public string? Icon { get; set; } = null;
        [Required]
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } = null;
    }
}
