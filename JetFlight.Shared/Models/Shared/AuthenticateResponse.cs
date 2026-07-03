namespace JetFlight.Shared.Models.Shared
{
    public class AuthenticateResponse
    {
        public string Token { get; set; }

        /// <summary>
        /// Refresh token for customer authentication (30-day lifetime)
        /// Only populated for customer logins, null for admin/cashdesk
        /// </summary>
        public string? RefreshToken { get; set; }

        public AuthenticateResponse(string token, string? refreshToken = null)
        {
            Token = token;
            RefreshToken = refreshToken;
        }
    }
}
