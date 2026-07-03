using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Admins
{
    public class AuthenticateEmailRequest
    {
        [Required(ErrorMessage = "Необхідно ввести пошту")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@(jetflight\.com|linkupst\.com)$",
        ErrorMessage = "Дозволений домен пошти: @jetflight.com")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Необхідно ввести пароль")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Пароль має містити мін 8 символів, 1 спец символ, мінімум 1 літеру верхнього і 1 літеру нижнього регістру")]
        public string Password { get; set; }
    }
}
