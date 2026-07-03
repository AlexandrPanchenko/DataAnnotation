using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Posts;

public class PostDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string Text { get; set; }
    public string Status { get; set; }
    public  int? OriginId { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string ReadDurationMin { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string ImageMimeType { get; set; }
    public string ImageName{ get; set; }
    public bool? FixedPost { get; set; }
    public string ImageSize { get; set; }
    public string ImageAlt { get; set; }
}
