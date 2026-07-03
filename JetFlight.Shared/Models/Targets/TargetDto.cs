using JetFlight.Shared.Models.Users;

namespace JetFlight.Shared.Models.Targets
{
    public class BaseTargetDto
    {        
        public byte? BranchId { get; set; }
        public List<int> CityIds { get; set; }

        // Age filters
        public RangeDTO<int?>? Age { get; set; }
        public RangeDTO<int?>? DayOfBirth { get; set; }
        public RangeDTO<Month?>? MonthOfBirth { get; set; }
        public RangeDTO<int?>? YearOfBirth { get; set; }

        public Sex? Sex { get; set; }

        // Shopping filters
        public RangeDTO<Month?>? ShoppingPeriod { get; set; }
        public RangeDTO<int?>? AverageCheckPositions { get; set; }
        public RangeDTO<decimal?>? AverageCheckAmount { get; set; }
        public RangeDTO<TimeSpan?>? ShoppingTime { get; set; }
        public FrequencyShoppingDTO? FrequencyShopping { get; set; }
        public RangeDTO<DateTime?>? RegisteredDate { get; set; }

        // Period filters
        public int? Period { get; set; }
        public RangeDTO<int?>? CheckCount { get; set; }
        public bool IncludeOnBirthdayOnly { get; set; }
        public RangeDTO<int?>? RegisteredDays { get; set; }

        // Product filters
        public RangeDTO<decimal?>? ProductAmount { get; set; }
        public List<string?>? CategoryCodes { get; set; }
        public List<string?>? ManufacturerCodes { get; set; }

        public List<int> RFMIds { get; set; }
        public required string Name { get; set; }
    }

    public class FrequencyShoppingDTO
    {
        public FrequencyType FrequencyType { get; set; }
        public int AverageTimes { get; set; }
    }

    public enum Month
    {
        January = 1,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }

    public enum FrequencyType
    {
        Day,
        Week,
        Month,
        Quarter
    }

    public class TargetDto : BaseTargetDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
