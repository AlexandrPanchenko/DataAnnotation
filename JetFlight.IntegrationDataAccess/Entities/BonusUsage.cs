namespace JetFlight.IntegrationDataAccess.Entities
{
    public class BonusUsage
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string TransactionNumber { get; set; }
        public string StoreCode { get; set; }
        public string PosTerminal { get; set; }
        public int CustomerBonusTransactionId { get; set; }
        public CustomerBonusTransaction CustomerBonusTransaction { get; set; }
    }
}
