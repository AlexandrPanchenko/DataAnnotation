using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.Users
{
    public class TestNotificationRequest
    {
        public int CustomerId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Type { get; set; }
    }
}
