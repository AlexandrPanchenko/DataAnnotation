using JetFlight.Shared.Models.Coupons;

namespace JetFlight.Shared.Models.LogHistory;

public class CouponLogHistoryDTO
{
    public string? Name { get; set; }
    public string? PrivateName { get; set; }
    public string? Description { get; set; }
    public string? PrivateDescription { get; set; }
    public string? Image { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }

    public EmissionBy? EmissionBy { get; set; }

    public List<CouponToTargetDto>? CouponToTarget { get; set; }
    public int? UseTimes { get; set; }

    public int? Emission { get; set; }

    public List<CouponToStoreDto>? CouponToStore { get; set; }

    public CouponClass? Class { get; set; }

    public CouponStatus? Status { get; set; }

    public CouponType? Type { get; set; }

    public CouponDetailsDTO? CouponDetails { get; set; }
}

public class CouponToTargetDto
{
    public int TargetId { get; set; }
}

public class AccumulationCardToTargetDto
{
    public int TargetId { get; set; }
}

public class CouponToStoreDto
{
    public string StoreCode { get; set; }
}