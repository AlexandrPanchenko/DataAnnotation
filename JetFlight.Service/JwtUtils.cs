using JetFlight.ApplicationDataAccess.Entities.DataContext;
using JetFlight.IntegrationDataAccess.Entities;
using JetFlight.Shared.Models.Store;
using JetFlight.Shared.UserContext;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JetFlight.Service;

public interface IJwtUtils
{
    public string GenerateJwtToken(Admin admin);
    string GenerateJwtToken(Customer customer, Branches branchId);
    string GenerateJwtToken(string clientId);
}

public class JwtUtils : IJwtUtils
{
    public string GenerateJwtToken(string clientId)
    {
        var claims = new List<Claim>
        {
            new("role", UserRole.Cashdesk.ToString()),
            new("clientId", clientId),
        };

        return GenerateJwtToken(claims);
    }

    public string GenerateJwtToken(Admin admin)
    {
        var claims = new List<Claim>
        {
            new("id", admin.Id.ToString()),
            new("first_name", admin.FirstName),
            new("last_name", admin.LastName),
            new("email", admin.Email),
            new("role", UserRole.Admin.ToString())
        };

        return GenerateJwtToken(claims);
    }

    public string GenerateJwtToken(Customer customer, Branches branchId)
    {
        var claims = new List<Claim>
        {
            new("id", customer.Id.ToString()),
            new("first_name", customer.FirstName ?? string.Empty),
            new("last_name", customer.LastName ?? string.Empty),
            new("phone_number", customer.PhoneNumber),
            new("role", UserRole.Customer.ToString()),
            new("branchId", branchId.ToString())
        };

        // 30 днів — менше запитів refresh, користувач залишається в системі
        return GenerateJwtToken(claims, expirationMinutes: 30 * 24 * 60);
    }

    private string GenerateJwtToken(List<Claim> claims, int? expirationMinutes = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT_SECRET_KEY environment variable is not set.");
        }
        var key = Encoding.ASCII.GetBytes(secretKey);

        // Default to 7 days for Admin, Cashdesk. Customer передає expirationMinutes явно.
        var expiration = expirationMinutes.HasValue
            ? DateTime.UtcNow.AddMinutes(expirationMinutes.Value)
            : DateTime.UtcNow.AddDays(7);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}