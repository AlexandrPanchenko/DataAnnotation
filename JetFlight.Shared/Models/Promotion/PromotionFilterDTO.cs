
namespace JetFlight.Shared.Models.Promotion
{
    public class PromotionFilterDTO
    {
        public int? offset { get; set; }
        public int? limit { get; set; }
        public string? searchParam { get; set; }
        public DateOnly? startedDate { get; set; }
        public string? cities { get; set; }
        public string? promotionTagIds { get; set; }
        public string? categoryCode { get; set; }
        public string? promotionTypeNavisionId { get; set; }
    }
}
