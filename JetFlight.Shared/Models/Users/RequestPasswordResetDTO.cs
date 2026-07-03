using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Admins
{
    public class RequestResetPasswordDTO
    {
        [Required(ErrorMessage = "Необхідно ввести пошту")]
        [AllowedEmailDomain(new[] { "jetflight.com", "linkupst.com" })]
        public string Email { get; set; }

    }
}
