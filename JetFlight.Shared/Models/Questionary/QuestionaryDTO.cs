using System.Text.Json.Serialization;

namespace JetFlight.Shared.Models.Questionary
{
    public class CreateQuestionaryDTO
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string Alt { get; set; }
        public byte? BranchId { get; set; }
        public QuestionaryRewardDTO Reward { get; set; }
        public int ActiveDaysAfterComplete { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<QuestionaryFieldDTO> Fields { get; set; }
    }

    [JsonDerivedType(typeof(QuestionaryBonusRewardDTO), nameof(QuestionaryBonusRewardDTO))]
    [JsonDerivedType(typeof(QuestionaryCouponRewardDTO), nameof(QuestionaryCouponRewardDTO))]
    public abstract class QuestionaryRewardDTO
    {

    }

    public class QuestionaryBonusRewardDTO : QuestionaryRewardDTO
    {
        public decimal Amount { get; set; }
    }

    public class QuestionaryCouponRewardDTO : QuestionaryRewardDTO
    {
        public int CouponId { get; set; }
    }

    public class UpdateQuestionaryDTO : CreateQuestionaryDTO
    {
        public int Id { get; set; }
    }

    public class CustomerQuestionaryDTO : UpdateQuestionaryDTO
    {
        public bool IsAnswered { get; set; }
        public bool IsLocked { get; set; }
    }

    public class AdminQuestionaryDTO : UpdateQuestionaryDTO
    {
        public QuestionaryStatus Status { get; set; }
        public bool IsLocked { get; set; }
        public int AnswerCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class QuestionaryFieldDTO
    {
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public QuestionaryItemType Type { get; set; }
        public Dictionary<string, string> Options { get; set; }
        public string? DefaultValue { get; set; }
        public string? Validation { get; set; }
    }
}
