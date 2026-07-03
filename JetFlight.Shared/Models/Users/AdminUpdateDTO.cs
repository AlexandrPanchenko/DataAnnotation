using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Admins
{
    public class AdminUpdateDTO
    {
        [Required]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Будь ласка введіть номер у форматі +380XXXXXXXXX")]
        public string? PhoneNumber { get; set; }
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
    ErrorMessage = "Пароль має містити мін 8 символів, 1 спец символ, мінімум 1 літеру верхнього і 1 літеру нижнього регістру")]
        public string? Password { get; set; }
        public string? Email { get; set; }
        public bool? Blocked { get; set; }
        [Required]
        public List<int> Roles { get; set; }
    }
}
