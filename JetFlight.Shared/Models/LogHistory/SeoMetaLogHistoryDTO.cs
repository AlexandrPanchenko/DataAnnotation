
namespace JetFlight.Shared.Models.LogHistory
{
    public class SeoMetaLogHistoryDTO
    {
        public int? Id { get; set; } = null;
        public string? EntityType { get; set; } = null;
        public int? EntityId { get; set; } = null;
        public string? Title { get; set; } = null;
        public string? Description { get; set; } = null;
    }
}
