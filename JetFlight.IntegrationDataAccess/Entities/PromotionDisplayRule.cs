using JetFlight.Shared.Models.Promotion;
using JetFlight.Shared.Models.Users;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class PromotionDisplayRule
    {
        public int Id { get; set; }
        public byte BranchId { get; set; }
        public int RelevantCount { get; set; }
        public int PerRuleCount { get; set; }

        public PromotionRulePeriod Period { get; set; }

        public PromotionDisplayRuleType Type { get; set; }

        public List<PromotionDisplayRuleToStore> Stores { get; set; }
        public List<PromotionDisplayRuleToActivityType> TypesOfActivity { get; set; }

        public int? AgeFrom { get; set; }
        public int? AgeTo { get; set; }

        public decimal? CheckAmountFrom { get; set; }
        public decimal? CheckAmountTo { get; set; }
    }

    public class PromotionDisplayRuleToStore
    {
        public int Id { get; set; }
        public string StoreCode { get; set; }

        public int PromotionDisplayRuleId { get; set; }
        public PromotionDisplayRule PromotionDisplayRule { get; set; }
    }

    public class PromotionDisplayRuleToActivityType
    {
        public int Id { get; set; }
        public CustomerTypeOfActivity TypeOfActivity { get; set; }

        public int PromotionDisplayRuleId { get; set; }
        public PromotionDisplayRule PromotionDisplayRule { get; set; }
    }
}
