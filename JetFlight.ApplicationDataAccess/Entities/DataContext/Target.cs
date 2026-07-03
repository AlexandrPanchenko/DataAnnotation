using JetFlight.Shared.Models.Targets;
using JetFlight.Shared.Models.Users;

namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class Target
    {
        public int Id { get; set; }
        public byte? BranchId { get; set; }
        public List<TargetToCity> Cities { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int? AgeFrom { get; set; }
        public int? AgeTo { get; set; }

        public int? DayOfBirthFrom { get; set; }
        public int? DayOfBirthTo { get; set; }
        public Month? MonthOfBirthFrom { get; set; }
        public Month? MonthOfBirthTo { get; set; }
        public int? YearOfBirthFrom { get; set; }
        public int? YearOfBirthTo { get; set; }

        public Month? ShoppingPeriodTo { get; set; }
        public Month? ShoppingPeriodFrom { get; set; }
        public int? AverageCheckPositionsFrom { get; set; }
        public int? AverageCheckPositionsTo { get; set; }
        public decimal? AverageCheckAmountFrom { get; set; }
        public decimal? AverageCheckAmountTo { get; set; }
        public TimeSpan? ShoppingTimeFrom { get; set; }
        public TimeSpan? ShoppingTimeTo { get; set; }
        public FrequencyType? FrequencyType { get; set; }
        public int? AverageFrequencyTimes { get; set; }
        public DateTime? RegisteredDateFrom { get; set; }
        public DateTime? RegisteredDateTo { get; set; }

        public int? Period { get; set; }
        public int? CheckCountFrom { get; set; }
        public int? CheckCountTo { get; set; }
        public bool IncludeOnBirthdayOnly { get; set; }
        public int? RegisteredDaysFrom { get; set; }
        public int? RegisteredDaysTo { get; set; }

        public decimal? ProductAmountFrom { get; set; }
        public decimal? ProductAmountTo { get; set; }
        public List<string> CategoryCodes { get; set; }
        public List<string> ManufacturerCodes { get; set; }

        public Sex? Sex { get; set; }

        public List<TargetToRFM> RFMs { get; set; }
    }

    public class TargetToCity
    {
        public int Id { get; set; }
        public int CityId { get; set; }
        public City City { get; set; }
        public int TargetId { get; set; }
        public Target Target { get; set; }
    }

    public class TargetToRFM
    {
        public int Id { get; set; }
        public int RFMId { get; set; }
        public RFM RFM { get; set; }
        public int TargetId { get; set; }
        public Target Target { get; set; }
    }
}
