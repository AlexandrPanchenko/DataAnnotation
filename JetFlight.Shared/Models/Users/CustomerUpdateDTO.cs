using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Avatars;
using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Users
{
    public class CustomerUpdateDTO
    {
        public int? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public AvatarKey? AvatarKey { get; set; }
        [RegularExpression(RegexConstants.Email)]
        public string? Email { get; set; }
        public DateTime? Birthday { get; set; }
        public CustomerTypeOfActivity? TypeOfActivity { get; set; }
        public Sex? Sex { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? WhereFindOut { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Street { get; set; }
        [RegularExpression(RegexConstants.Latitude)]
        public string? Latitude { get; set; }
        [RegularExpression(RegexConstants.Longitude)]
        public string? Longitude { get; set; }
        public int? StoreId { get; set; }
        public bool? IsBlocked { get; set; }
        public bool? EnablePushNotifications { get; set; }
        public string? PushNotificationToken { get; set; }
        public bool? EnableEmailNotifications { get; set; }
        public bool? EnableSmsNotifications { get; set; }
        public bool? EnableSubscription { get; set; }
        public bool? AutomaticWithdrawal { get; set; }
        public bool? AccumulateRest { get; set; }
        public int? StoreNearHomeId { get; set; }
        public DateTime? PersonalQuestionaryCompletedAt { get; set; }
        public DateTime? DateOfRegistration { get; set; }
    }
}
