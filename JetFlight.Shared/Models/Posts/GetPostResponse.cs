using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Posts
{
    public class GetPostResponse
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string ImageMimeType { get; set; }
        [Required]
        public string ImagePath { get; set; }
        [Required]
        public string ImageSize { get; set; }
        [Required]
        public string ImageAlt { get; set; }
        [Required]
        public string Subtitle { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime? PublishedAt { get; set; } = null;
        [Required]
        public bool Status { get; set; }
        public byte? BranchId { get; set; }

        [Required]
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } = null;
    }
}
