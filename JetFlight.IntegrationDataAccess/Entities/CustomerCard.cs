using JetFlight.Shared.Models.Users;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class CustomerCard
    {
        public int? CustomerId { get; set; }
        public byte BranchId { get; set; }
        public string Code { get; set; } = default!;
        public bool IsBlocked { get; set; }
        public CardType Type { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Customer Customer { get; set; }
        public ICollection<CustomerBonusTransaction> CustomerBonusTransactions { get; set; }
    }
}
