using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.LogHistory
{
    public class LogHistoryField
    {
        public LogHistoryField() { }
        public LogHistoryField(string title, string fromValue, string toValue)
        {
            this.Title = title;
            this.FromValue = fromValue;
            this.ToValue = toValue;
        }

        public string? Title { get; set; } = null;
        public string? FromValue { get; set; } = null;
        public string? ToValue { get; set; } = null;
    }
}
