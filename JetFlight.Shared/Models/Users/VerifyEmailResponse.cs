namespace JetFlight.Shared.Models.Users
{
    public class VerifyEmailResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal? BonusAmount { get; set; }
        public string? RedirectUrl { get; set; }
    }
}

