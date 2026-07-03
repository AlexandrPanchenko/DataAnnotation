namespace JetFlight.IntegrationDataAccess.Entities
{
    public class PromotionQueue
    {
        public int Id { get; set; }
        public int PromotionId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
