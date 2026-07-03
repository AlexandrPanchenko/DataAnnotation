using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetFlight.Shared.Models.ContactUs
{
    public class ContactUsChangeStatusAndAssigneResponse
    {
        public bool Response { get; set; }
        public List<string> Errors { get; set; }

        public ContactUsChangeStatusAndAssigneResponse()
        {
            Errors = new List<string>();
        }
    }
}
