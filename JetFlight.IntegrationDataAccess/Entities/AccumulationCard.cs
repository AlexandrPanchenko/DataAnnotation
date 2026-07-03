using JetFlight.Shared.Models.AccumulationCard;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class AccumulationCard : IAuditable, IRelatedAuditable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public int CountToComplete { get; set; }
        public bool AllRequired { get; set; }
        public string Description { get; set; }
        public List<AccumulationCardToTarget> Targets { get; set; }
        public List<Coupon> Coupons { get; set; }
        public List<CustomerAccumulationCard> CustomerAccumulationCards { get; set; }
        public AccumulationCardStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CustomerAccumulationCard
    {
        public int Id { get; set; }
        public int Counter { get; set; }
        public int CustomerId { get; set; }
        public CustomerAccumulationCardStatus Status { get; set; }
        public Customer Customer { get; set; }
        public int AccumulationCardId { get; set; }
        public AccumulationCard AccumulationCard { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? UsedAt { get; set; }
    }

    public class AccumulationCardToTarget
    {
        public int Id { get; set; }
        public int AccumulationCardId { get; set; }
        public AccumulationCard AccumulationCard { get; set; }
        public int TargetId { get; set; }
    }
}
