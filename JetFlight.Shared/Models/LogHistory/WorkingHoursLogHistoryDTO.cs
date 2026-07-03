using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.LogHistory
{
    public class WorkingHoursLogHistoryDTO
    {
        public int? WorkingHoursId { get; set; } = null;
        public Day? Day { get; set; } = null;
        public DateTime? Date { get; set; } = null;
        public TimeSpan? OpeningTime { get; set; } = null;
        public TimeSpan? ClosingTime { get; set; } = null;
        public bool? IsActive { get; set; } = null;
        public string? Note { get; set; } = null;

    }
}
