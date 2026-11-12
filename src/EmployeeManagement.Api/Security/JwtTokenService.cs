using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagement.Api.Security;

public sealed record TokenResponse(string AccessToken, string TokenType, DateTimeOffset ExpiresAt);

public interface ITokenService
{
    TokenResponse CreateToken(string userName, IReadOnlyCollection<string> roles);
}

// Issues HMAC-SHA256 signed JWTs. A real system would get tokens from an identity provider
// (Entra ID, Auth0, Keycloak...) and the API would only validate them.
public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider time) : ITokenService
{
    public const string RoleClaim = "role";
    public const string NameClaim = "name";

    public TokenResponse CreateToken(string userName, IReadOnlyCollection<string> roles)
    {
        JwtOptions jwt = options.Value;
        DateTimeOffset now = time.GetUtcNow();
        DateTimeOffset expires = now.AddMinutes(jwt.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userName),
            new(NameClaim, userName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };
        claims.AddRange(roles.Select(role => new Claim(RoleClaim, role)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            SigningCredentials = new SigningCredentials(CreateSigningKey(jwt.SigningKey), SecurityAlgorithms.HmacSha256),
        };

        return new TokenResponse(new JsonWebTokenHandler().CreateToken(descriptor), "Bearer", expires);
    }

    public static SymmetricSecurityKey CreateSigningKey(string signingKey) => new(Encoding.UTF8.GetBytes(signingKey));
}
