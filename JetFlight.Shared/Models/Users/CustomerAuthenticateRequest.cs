using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Users
{
    public class CustomerAuthenticateRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }


        [Required]
        [RegularExpression("^\\d{4}$")]
        public string Code { get; set; }
    }
}
