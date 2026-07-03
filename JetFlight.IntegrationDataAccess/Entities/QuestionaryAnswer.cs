using JetFlight.Shared.Models;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class QuestionaryAnswer
    {
        public int Id { get; set; }
        public Questionary Questionary { get; set; }
        public int QuestionaryId { get; set; }
        public Customer Customer { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public byte BranchId { get; set; }
        public ClientPlatform ClientPlatform { get; set; }

        public List<QuestionaryAnswerField<int>> IntAnswers { get; set; }
        public List<QuestionaryAnswerField<float>> FloatAnswers { get; set; }
        public List<QuestionaryAnswerField<string>> StringAnswers { get; set; }
        public List<QuestionaryAnswerField<DateTime>> DateTimeAnswers { get; set; }
        public List<QuestionaryAnswerField<QuestionarySelectOption>> SingleSelectAnswers { get; set; }
        public List<QuestionaryAnswerField<List<QuestionarySelectOption>>> MultiSelectAnswers { get; set; }
    }

    public class QuestionaryAnswerField<T>
    {
        public int Id { get; set; }
        public QuestionaryField QuestionaryField { get; set; }
        public int QuestionaryFieldId { get; set; }
        public T Answer { get; set; }
    }
}
