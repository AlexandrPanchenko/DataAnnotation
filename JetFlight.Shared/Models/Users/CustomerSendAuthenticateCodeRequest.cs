using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Users
{
    public class CustomerSendAuthenticateCodeRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } 
    }
}
