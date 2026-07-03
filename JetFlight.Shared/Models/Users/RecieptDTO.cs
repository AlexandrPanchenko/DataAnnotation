
using JetFlight.Shared.Models.Coupons;
using JetFlight.Shared.Models.Store;

namespace JetFlight.Shared.Models.Users;

public class ReceiptDTO
{
    public int Id { get; set; }
    public string? CardCode { get; set; }
    public byte BranchId { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Discount { get; set; }
    public StoreDTO? StoreDetails { get; set; }
    public List<ReceiptProductDTO> ReceiptProducts { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal AccumulatedBonuses { get; set; }
    public decimal SpentBonuses { get; set; }
    public List<UsedCouponDTO> UsedCoupons { get; set; }
    public decimal TotalPriceWithDiscount { get; set; }
    public bool IsReturn { get; set; }
}
