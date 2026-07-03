using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class CustomerNotification
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int? BranchId { get; set; }
        public int? Type { get; set; }
        public bool? Read { get; set; }
        public bool? Received { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Customer Customer { get; set; }
    }
}
