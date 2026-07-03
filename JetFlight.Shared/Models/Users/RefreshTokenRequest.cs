using System.ComponentModel.DataAnnotations;

namespace JetFlight.Shared.Models.Users
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public required string RefreshToken { get; set; } = default!;
    }
}
