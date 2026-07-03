using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Subscription
{
    public class SubscriptionRequest
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
}
