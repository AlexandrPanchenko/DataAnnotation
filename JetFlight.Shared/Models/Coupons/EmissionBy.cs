using JetFlight.Shared.Extensions;

namespace JetFlight.Shared.Models.Coupons
{
    public enum EmissionBy
    {
        [EnumDisplay("Випадково")]
        Random,
        [EnumDisplay("За зростанням суми чеків")]
        CheckAmountAsc,
        [EnumDisplay("За спаданням суми чеків")]
        CheckAmountDesc,
        [EnumDisplay("За зростанням частити покупок")]
        CheckQuantityAsc,
        [EnumDisplay("За спаданням частити покупок")]
        CheckQuantityDesc,
    }
}
