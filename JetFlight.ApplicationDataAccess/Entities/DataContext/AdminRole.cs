using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class AdminToRole
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public int RoleId { get; set; }

        public Admin Admin { get; set; }
        public AdminRole Role { get; set; }
    }
}
