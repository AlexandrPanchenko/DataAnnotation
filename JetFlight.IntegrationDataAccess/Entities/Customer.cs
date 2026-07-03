using JetFlight.Shared.Models.Users;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? Birthday { get; set; }
        public Sex? Sex { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? WhereFindOut { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsDeleted { get; set; }
        public RegistrationPlatform? RegistrationPlatform { get; set; }
        public CustomerTypeOfActivity? TypeOfActivity { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? StoreNearHomeId { get; set; }
        public DateTime? PersonalQuestionaryCompletedAt { get; set; }
        public bool EmailVerified { get; set; }
        public int? NumberOfChildren { get; set; }
        public ICollection<CustomerDevice> CustomerDevices { get; set; }
        public ICollection<CustomerNotification> CustomerNotifications { get; set; }
        public ICollection<CustomerSetting> CustomerSettings { get; set; }
        public ICollection<CustomerBonusTransaction> CustomerBonusTransactions { get; set; }
        public ICollection<CustomerCard> CustomerCards { get; set; }
    }
}
