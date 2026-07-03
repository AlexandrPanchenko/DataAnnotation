using JetFlight.Shared.Models.Avatars;

namespace JetFlight.Shared.Models.Feedback
{
    public class FeedbackCustomerDTO
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public AvatarDTO Avatar { get; set; }
    }
}
