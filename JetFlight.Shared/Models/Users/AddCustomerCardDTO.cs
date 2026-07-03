using JetFlight.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Users
{
    public class AddCustomerCardDTO
    {
        [Required]
        [RegularExpression(RegexConstants.PhysicalCard)]
        public string Code { get; set; }
    }
}
