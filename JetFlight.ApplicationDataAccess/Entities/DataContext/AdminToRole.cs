using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class AdminRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public bool isActive { get; set; }

        public List<RoleToPermission> RoleToPermissions { get; set; } = new List<RoleToPermission>();
    }
}
