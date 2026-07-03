using JetFlight.Shared.Models;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class ReceiptProduct
    {
        public int Id { get; set; }
        public int ReceiptId { get; set; }
        public Receipt Receipt { get; set; }
        public string ProductCode { get; set; }
        public Product Product { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal Quantity { get; set; }
        public ItemUnit ItemUnit { get; set; }
        public int LineNo { get; set; }
        public decimal LineTotalAmount { get; set; }
        public decimal LineTotalAmountWithDiscount { get; set; }
    }
}
