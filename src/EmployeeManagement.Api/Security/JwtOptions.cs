using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Security;

// Issuer and audience live in appsettings.json. The signing key is a secret: it comes from
// user-secrets in development and an environment variable or vault elsewhere, never from source control.
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = "";

    [Required]
    public string Audience { get; set; } = "";

    [Required(ErrorMessage = "Jwt:SigningKey is not configured. See README > Authentication for how to set it.")]
    [MinLength(32, ErrorMessage = "Jwt:SigningKey must be at least 32 characters (256 bits) for HMAC-SHA256.")]
    public string SigningKey { get; set; } = "";

    [Range(1, 1440)]
    public int ExpiryMinutes { get; set; } = 60;
}
