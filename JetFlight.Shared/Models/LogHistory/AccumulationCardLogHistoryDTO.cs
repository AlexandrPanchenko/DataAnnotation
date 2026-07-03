using JetFlight.Shared.Models.AccumulationCard;

namespace JetFlight.Shared.Models.LogHistory;

public class AccumulationCardLogHistoryDTO
{
    public string? Name { get; set; }

    public string? Icon { get; set; }

    public int? CountToComplete { get; set; }

    public bool? AllRequired { get; set; }

    public string? Description { get; set; }

    public List<AccumulationCardToTargetDto>? AccumulationCardToTarget { get; set; }

    public AccumulationCardStatus? Status { get; set; }

    public string? Image { get; set; }

    public string? CouponDescription { get; set; }

    public List<string>? ProductCodes { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? ExpirationDate { get; set; }
}