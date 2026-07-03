using JetFlight.Shared.Models.Avatars;
using System.Text.Json.Serialization;

namespace JetFlight.Shared.Models.Questionary
{
    public class QuestionaryAnswerDTO
    {
        public int QuestionaryId { get; set; }
        public Dictionary<string, QuestionaryFieldAnswerDTO> Answers { get; set; }
    }

    public class CustomerQuestionaryAnswerDTO : QuestionaryAnswerDTO
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAvatar { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte BranchId { get; set; }
        public ClientPlatform ClientPlatform { get; set; }
    }

    [JsonDerivedType(typeof(QuestionaryStringAnswerDTO), nameof(QuestionaryStringAnswerDTO))]
    [JsonDerivedType(typeof(QuestionaryIntAnswerDTO), nameof(QuestionaryIntAnswerDTO))]
    [JsonDerivedType(typeof(QuestionaryFloatAnswerDTO), nameof(QuestionaryFloatAnswerDTO))]
    [JsonDerivedType(typeof(QuestionaryDateTimeAnswerDTO), nameof(QuestionaryDateTimeAnswerDTO))]
    [JsonDerivedType(typeof(QuestionarySelectAnswerDTO), nameof(QuestionarySelectAnswerDTO))]
    [JsonDerivedType(typeof(QuestionaryMultiSelectAnswerDTO), nameof(QuestionaryMultiSelectAnswerDTO))]
    public abstract class QuestionaryFieldAnswerDTO
    {
    }

    public class QuestionarySelectAnswerDTO : QuestionaryFieldAnswerDTO
    {
        public string Key { get; set; }
        public string? Value { get; set; }
    }

    public class QuestionaryMultiSelectAnswerDTO : QuestionaryFieldAnswerDTO
    {
        public List<QuestionarySelectAnswerDTO> Items { get; set; }
    }

    public class QuestionaryStringAnswerDTO : QuestionaryFieldAnswerDTO
    {
        public string Value { get; set; }
    }

    public class QuestionaryIntAnswerDTO : QuestionaryFieldAnswerDTO
    {
        public int Value { get; set; }
    }

    public class QuestionaryFloatAnswerDTO : QuestionaryFieldAnswerDTO
    {
        public float Value { get; set; }
    }

    public class QuestionaryDateTimeAnswerDTO : QuestionaryFieldAnswerDTO
    {
        public DateTime Value { get; set; }
    }
}
