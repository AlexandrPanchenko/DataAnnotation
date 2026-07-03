using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class Subscription
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }  // Consider using an Enum type for subscription type
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime? UnsubscribedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // public Customer Customer { get; set; }  // Navigation Property
    }
}
