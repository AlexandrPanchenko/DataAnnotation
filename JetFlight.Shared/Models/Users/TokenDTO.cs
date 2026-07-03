using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Admins
{
    public class TokenDTO
    {
        [Required(ErrorMessage = "Необхідно ввести код")]
        public string AuthCode { get; set; }
    }
}
