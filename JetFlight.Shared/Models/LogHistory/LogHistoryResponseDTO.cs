using JetFlight.Shared.Models.Roles;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.LogHistory
{
    public class LogHistoryResponseDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<RoleDTO> Roles { get; set; } = default!;
        public List<LogHistoryDTO> Logs { get; set; } = default!;
    }
}
