using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetFlight.Shared.Models.Store;

namespace JetFlight.Shared.Models.LogHistory
{
    public class LogHistoryDTO
    {
        public string EntityType { get; set; }
        public int? EntityId { get; set; }
        public string Action { get; set; }
        public DateTime? Date { get; set; }

        public int? ActionBy { get; set; }

        public string ActionByName { get; set; }

        public List<LogHistoryField> LogHistoryList { get; set; }
    }
}
