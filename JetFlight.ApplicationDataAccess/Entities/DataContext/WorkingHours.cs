using JetFlight.Shared.Models;

namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class WorkingHours
    {
        public int WorkingHoursId { get; set; }
        public Day? Day { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public bool IsActive { get; set; }
        public string Note { get; set; }
        public int StoreId { get; set; }
        // Navigation property
        public Store Store { get; set; }

    }
}
