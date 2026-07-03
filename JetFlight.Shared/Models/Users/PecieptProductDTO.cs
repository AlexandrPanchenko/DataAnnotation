
using JetFlight.Shared.Models.Coupons;

namespace JetFlight.Shared.Models.Users;

public class ReceiptProductDTO
{
    public int Id { get; set; }
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public string ProductImage { get; set; }
    public decimal Price { get; set; }
    public string ItemUnit { get; set; }
    public decimal Discount { get; set; }
    public decimal Quantity { get; set; }
    public int? CustomerCouponId => UsedCoupon?.Id;
    public UsedCouponDTO? UsedCoupon { get; set; }
    public decimal LineTotalAmount { get; set; }
    public decimal LineTotalAmountWithDiscount { get; set; }
}
