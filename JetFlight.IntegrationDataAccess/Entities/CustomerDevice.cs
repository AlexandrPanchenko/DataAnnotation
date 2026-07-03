using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.IntegrationDataAccess.Entities
{
    public class CustomerDevice
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public int? BranchId { get; set; }
        public string DeviceId { get; set; }
        public string DeviceInfo { get; set; }
        public int? DevicePlatformId { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceName { get; set; }
        public DateTime? LastActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Customer Customer { get; set; }
    }
}
