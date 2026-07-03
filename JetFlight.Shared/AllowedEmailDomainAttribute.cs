using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace JetFlight.Shared;
public class AllowedEmailDomainAttribute : ValidationAttribute
{
    private readonly string[] _allowedDomains;

    public AllowedEmailDomainAttribute(string[] allowedDomains)
    {
        _allowedDomains = allowedDomains;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is string email)
        {
            string pattern = @"^[^@\s]+@(" + string.Join("|", _allowedDomains) + ")$";
            if (Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500)))
            {
                return ValidationResult.Success;
            }
        }

        return new ValidationResult($"Дозволений домен пошти: @jetflight.com ");
    }
}

