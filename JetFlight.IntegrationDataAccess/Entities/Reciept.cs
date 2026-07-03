namespace JetFlight.IntegrationDataAccess.Entities
{
    public class Receipt
    {
        public int Id { get; set; }
        public string? CardCode { get; set; }

        public string TransactionNumber { get; set; }
        public string PosTerminal { get; set; }
        public string ReceiptNumber { get; set; }
        public bool ? IsActive { get; set; }
        public bool IsReturn { get; set; }

        public CustomerCard CustomerCard { get; set; }
        public List<ReceiptProduct> ReceiptProducts { get; set; }
        public byte BranchId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StoreCode { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountWithDiscount { get; set; }
        public decimal AccumulatedBonusesSnapshot { get; set; }
        public decimal UsedBonusesSnapshot { get; set; }
        public int? CustomerBonusTransactionId { get; set; }
        public CustomerBonusTransaction CustomerBonusTransaction { get; set; }

        public List<ReceiptCustomerCoupon> ReceiptCustomerCoupons { get; set; }
    }

    public class ReceiptCustomerCoupon
    {
        public int Id { get; set; }
        public int CustomerCouponId { get; set; }
        public CustomerCoupon CustomerCoupon { get; set; }
        public int LineNo { get; set; }

        public int ReceiptId { get; set; }
        public Receipt Receipt { get; set; }
    }
}
