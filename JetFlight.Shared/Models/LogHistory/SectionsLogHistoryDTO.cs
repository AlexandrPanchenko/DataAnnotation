
namespace JetFlight.Shared.Models.LogHistory
{
    public class SectionsLogHistoryDTO
    {
        public int? Id { get; set; } = null;
        public int? PageId { get; set; } = null;
        public string? Name { get; set; } = null;
        public string? Title { get; set; } = null;

        public List<SectionFieldLogHistoryDTO>? SectionFields = null;
    }
}
