using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.LogHistory
{
    public class LogHistoryItem<T, TKey> where TKey : notnull
    {
        public TKey Key { get; set; }

        public T? Right { get; set; }

        public T? Left { get; set; }

    }
}
