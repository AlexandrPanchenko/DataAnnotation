using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.Users
{
    public class GetAllCustomersResponse
    {
        public int Total { get; set; }
        public List<AdminCustomerDTO> Items { get; set; }
    }
}
