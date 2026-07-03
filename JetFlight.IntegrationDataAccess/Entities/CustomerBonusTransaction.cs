namespace JetFlight.IntegrationDataAccess.Entities
{
    public class CustomerBonusTransaction
    {
        public int Id { get; set; }
        public int? BranchId { get; set; }
        public string CardCode { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountRemaining { get; set; }
        public DateTime TransactionDate { get; set; }
        public int? TransactionType { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public CustomerCard CustomerCard { get; set; }
    }
}
