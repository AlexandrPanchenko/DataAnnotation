namespace JetFlight.Shared.Models.Loyalty
{
    public class UseBonusesDto
    {
        public string CardCode { get; set; }  = default!;
        public decimal Amount { get; set; }
        public string TransactionNumber { get; set; }
        public string StoreCode { get; set; }
        public string PosTerminal { get; set; }
    }
}
