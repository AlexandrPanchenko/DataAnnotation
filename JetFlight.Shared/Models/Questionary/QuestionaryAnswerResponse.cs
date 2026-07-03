namespace JetFlight.Shared.Models.Questionary
{
    public class QuestionaryAnswerResponse
    {
        public bool RequiresEmailVerification { get; set; }
        public string? EmailVerificationMessage { get; set; }
        public string? Email { get; set; }
    }
}
