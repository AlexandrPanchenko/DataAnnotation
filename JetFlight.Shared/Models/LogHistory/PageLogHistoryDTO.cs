
namespace JetFlight.Shared.Models.LogHistory
{
    public class PageLogHistoryDTO
    {
        public int? Id { get; set; } = null;
        public byte? StoreChain { get; set; } = null;
        public bool? Published { get; set; } = null;
        public int? OriginId { get; set; } = null;
        public int? ParentId { get; set; } = null;
        public string? Name { get; set; } = null;
        public string? Title { get; set; } = null;
        public string? Link { get; set; } = null;
        public bool? IsActive { get; set; } = null;
        public DateTime? ScheduledPublishDate { get; set; } = null;
        public DateTime? PublishedAt { get; set; } = null;
    }
}
