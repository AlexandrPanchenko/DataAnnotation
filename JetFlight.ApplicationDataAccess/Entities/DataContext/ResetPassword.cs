using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.ApplicationDataAccess.Entities.DataContext
{
    public class ResetPassword
    {
        [Key]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string AuthCode { get; set; }
        public Admin Admin { get; set; }
        public int? AdminId { get; set; }
        public DateTime CreatedDateTime { get; set; }

    }
}
