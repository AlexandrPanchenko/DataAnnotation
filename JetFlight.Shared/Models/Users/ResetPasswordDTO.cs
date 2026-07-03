using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Admins
{
    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "Необхідно ввести пароль")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Пароль має містити мін 8 символів, 1 спец символ, мінімум 1 літеру верхнього і 1 літеру нижнього регістру")]
        public string NewPassword { get; set; }
        [Required]
        public string AuthCode { get; set; }
    }
}
