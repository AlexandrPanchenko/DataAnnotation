using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.LogHistory
{
    public class LogHistoryMessage
    {
        public List<LogMessage> Logs { get; set; } = new ();
    }

    public class LogMessage
    {
        public int? AdminId { get; set; }
        public string EntityType { get; set; }
        public string? UpdatedFrom { get; set; }
        public string? UpdatedTo { get; set; }
        public int? EntityId { get; set; }
        public string Action { get; set; }
        public DateTime? Date { get; set; }
    }
}
