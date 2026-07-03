using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.Store
{
    public class WorkingHoursDTO
    {
        public int? WorkingHoursId { get; set; } = null;
        public Day? Day { get; set; } = null;
        public DateTime? Date { get; set; } = null;
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public bool IsActive { get; set; }
        public string? Note { get; set; } = null;

    }
}
