using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.Roles
{
    public class GetRolesResponse
    {
        public int Total { get; set; }
        public List<GetRoleFullResponse>? Roles { get; set; } = default!;
    }
}
