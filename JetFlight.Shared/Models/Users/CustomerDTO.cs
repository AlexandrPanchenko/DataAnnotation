using JetFlight.Shared.Models.Avatars;
using System.ComponentModel.DataAnnotations;
using JetFlight.Shared.Models.Store;



namespace JetFlight.Shared.Models.Users
{
    public class CustomerDTO
    {
        [Required]
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateTime? Birthday { get; set; }
        public Sex? Sex { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public CustomerTypeOfActivity? TypeOfActivity { get; set; }
        public List<CustomerCardDTO> Cards { get; set; }
        public CustomerSettingsDTO Setting { get; set; }
        public decimal AvailableBonuses { get; set; }
        public decimal UsedBonuses { get; set; }
        public int? StoreNearHomeId { get; set; }
        public DateTime? PersonalQuestionaryCompletedAt { get; set; }
        public string? WhereFindOut { get; set; }
    }

    public class CustomerSettingsDTO
    {
        public byte BranchId { get; set; }
        public AvatarDTO Avatar { get; set; }
        public int? ActiveStoreId { get; set; }
        public bool? EnablePushNotifications { get; set; }
        public string? PushNotificationToken { get; set; }
        public bool? EnableEmailNotifications { get; set; }
        public bool? EnableSmsNotifications { get; set; }
        public bool? EnableSubscription { get; set; }
        public bool? AutomaticWithdrawal { get; set; }
        public bool? AccumulateRest { get; set; }
    }

    public class CustomerCardDTO
    {
        public string Code { get; set; }

        public bool IsBlocked { get; set; }

        public CardType Type { get; set; }

        public Branches BranchId { get; set; }
    }
}
