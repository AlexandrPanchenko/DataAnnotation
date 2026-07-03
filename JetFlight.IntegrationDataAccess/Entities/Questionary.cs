using JetFlight.Shared.Models.Questionary;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class Questionary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Alt { get; set; }
        public byte? BranchId { get; set; }
        public decimal? BonusReward { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ActiveDaysAfterComplete { get; set; }
        public DateTime ExpirationDate { get; set; }
        public QuestionaryStatus Status { get; set; }
        public List<QuestionaryField> Fields { get; set; }
        public List<QuestionaryAnswer> Answers { get; set; }
        public bool IsLocked { get; set; }
        public int? CouponId { get; set; }
        public Coupon Coupon { get; set; }
    }

    public class QuestionaryField
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public int? QuestionaryId { get; set; }
        public Questionary? Questionary { get; set; }
        public QuestionaryItemType Type { get; set; }
        public List<QuestionarySelectOption> Options { get; set; }
        public string? Validation { get; set; }
    }

    public class QuestionarySelectOption
    {
        public int Id { get; set; }
        public int QuestionaryFieldId { get; set; }
        public QuestionaryField QuestionaryField { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
