using JetFlight.Shared.Models.Store;
using JetFlight.Shared.Models.Users;
using System.Text.Json.Serialization;

namespace JetFlight.Shared.Models.Promotion
{
    public class PromotionDisplayRuleDTO
    {
        public Branches BranchId { get; set; }
        public int RelevantCount { get; set; }
        public int PerRuleCount { get; set; }
        public PromotionRulePeriod Period { get; set; }
        public PromotionDisplayBaseRuleDTO Rule { get; set; }
    }

    [JsonDerivedType(typeof(PromotionDisplayLocationRuleDTO), nameof(PromotionDisplayLocationRuleDTO))]
    [JsonDerivedType(typeof(PromotionDisplayAgeRuleDTO), nameof(PromotionDisplayAgeRuleDTO))]
    [JsonDerivedType(typeof(PromotionDisplayTypeOfActivityRuleDTO), nameof(PromotionDisplayTypeOfActivityRuleDTO))]
    [JsonDerivedType(typeof(PromotionDisplayAverageCheckRuleDTO), nameof(PromotionDisplayAverageCheckRuleDTO))]
    public abstract class PromotionDisplayBaseRuleDTO
    {

    }

    public class PromotionDisplayLocationRuleDTO : PromotionDisplayBaseRuleDTO
    {
        public List<int> StoreIds { get; set; }
    }

    public class PromotionDisplayAgeRuleDTO : PromotionDisplayBaseRuleDTO
    {
        public RangeDTO<int> Age { get; set; }
    }

    public class PromotionDisplayTypeOfActivityRuleDTO : PromotionDisplayBaseRuleDTO
    {
        public HashSet<CustomerTypeOfActivity> TypesOfActivity { get; set; }
    }

    public class PromotionDisplayAverageCheckRuleDTO : PromotionDisplayBaseRuleDTO
    {
        public RangeDTO<decimal> Amount { get; set; }
    }

    public enum PromotionDisplayRuleType
    {
        Location,
        Age,
        TypeOfActivity,
        AverageCheck,
    }

    public enum PromotionRulePeriod
    {
        Quarter,
        HalfYear,
    }
}
