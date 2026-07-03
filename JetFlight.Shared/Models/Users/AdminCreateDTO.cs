using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Admins
{
    public class AdminCreateDTO
    {
        [Required(ErrorMessage = "Необхідно ввести імя")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Необхідно ввести призвіще")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Необхідно ввести пошту")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@(jetflight\.com|linkupst\.com)$",
        ErrorMessage = "Дозволений домен пошти: @jetflight.com")]
        public string Email { get; set; }
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Будь ласка введіть номер у форматі +380XXXXXXXXX")]

        public string? PhoneNumber { get; set; }

        public List<int> Roles { get; set; }
    }
}
