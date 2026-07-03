

namespace JetFlight.Shared.Models.LogHistory
{
    public class PostLogHistoryDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; } 
        public string? Description { get; set; } 
        public string Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Text { get; set; }
        public int? BranchId { get; set; }
        public string? Status { get; set; }
        public int? OriginId { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? ReadDurationMin { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ImageName { get; set; }
        public bool? FixedPost { get; set; }
        public bool? IsActive { get; set; }
        public string? ImageAlt { get; set; }
    }
}
