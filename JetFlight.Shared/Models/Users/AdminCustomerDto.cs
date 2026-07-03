using JetFlight.Shared.Models.Store;
using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Users
{
    public class AdminCustomerDTO
    {
        [Required]
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime? DateOfRegistration { get; set; }
        public Sex? Sex { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public CustomerTypeOfActivity? TypeOfActivity { get; set; }
        public List<AdminCustomerCardDTO> Cards { get; set; }
        public RegistrationPlatform? RegistrationPlatform { get; set; }
        public CustomerStatus CustomerStatus { get; set; }
        public CustomerSettingsDTO Setting { get; set; }
        public decimal AvailableBonuses { get; set; }
        public decimal UsedBonuses { get; set; }
        public int? StoreNearHomeId { get; set; }
        public DateTime? PersonalQuestionaryCompletedAt { get; set; }
        public string? WhereFindOut { get; set; }
        public bool EmailVerified { get; set; }
        public int? NumberOfChildren { get; set; }
    }

    public class AdminCustomerCardDTO
    {
        public string Code { get; set; }

        public bool IsBlocked { get; set; }
        public CardType Type { get; set; }
        public Branches BranchId { get; set; }
        public List<AdminCustomerTransactionDTO> AdminCustomerTransactionsDTO { get; set; }
    }

    public class AdminCustomerTransactionDTO
    {
        public int Id { get; set; }
        public int? BranchId { get; set; }
        public string CardCode { get; set; }
        public string Description { get; set; }
        public decimal? Amount { get; set; }
        public decimal? AmountRemaining { get; set; }
        public DateTime? TransactionDate { get; set; }
        public int? TransactionType { get; set; }
        public DateTime? ExpiredAt { get; set; }
    }
}
