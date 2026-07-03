using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class Admin
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string PhoneNumber { get; set; }
        public int? CreatedBy { get; set; }
        public bool? IsSuperadmin { get; set; }
        public bool? Blocked { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Admin CreatedByAdmin { get; set; }
    }
}
