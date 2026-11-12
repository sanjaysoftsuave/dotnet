using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Api.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagement.Api.Tests.Security;

public class JwtTokenServiceTests
{
    private const string Key = "unit-test-signing-key-that-is-long-enough-0123";
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 9, 0, 0, TimeSpan.Zero);

    private readonly JwtOptions _options = new() { Issuer = "issuer", Audience = "audience", SigningKey = Key, ExpiryMinutes = 30 };

    [Fact]
    public void CreateToken_ContainsUserRolesIssuerAndExpiry()
    {
        // Act
        TokenResponse response = CreateService().CreateToken("asha", [Roles.HrManager]);
        var token = new JsonWebToken(response.AccessToken);

        // Assert
        Assert.Equal("asha", token.Subject);
        Assert.Equal(Roles.HrManager, token.GetClaim(JwtTokenService.RoleClaim).Value);
        Assert.Equal("issuer", token.Issuer);
        Assert.Equal(Now.AddMinutes(30), response.ExpiresAt);
        Assert.Equal(Now.AddMinutes(30).UtcDateTime, token.ValidTo);
    }

    [Fact]
    public async Task CreateToken_ValidatesWithSameKey_FailsWithDifferentKey()
    {
        // Arrange
        string token = CreateService().CreateToken("asha", [Roles.Viewer]).AccessToken;
        var handler = new JsonWebTokenHandler();

        // Act
        TokenValidationResult good = await handler.ValidateTokenAsync(token, Parameters(Key));
        TokenValidationResult tampered = await handler.ValidateTokenAsync(token, Parameters("another-key-that-is-also-long-enough-987654"));

        // Assert
        Assert.True(good.IsValid);
        Assert.False(tampered.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("too-short")]
    public void JwtOptions_MissingOrShortKey_FailsValidation(string key)
    {
        var options = new JwtOptions { Issuer = "i", Audience = "a", SigningKey = key };

        Assert.False(Validator.TryValidateObject(options, new ValidationContext(options), [], validateAllProperties: true));
    }

    private JwtTokenService CreateService() => new(Options.Create(_options), new FakeTimeProvider(Now));

    private TokenValidationParameters Parameters(string key) => new()
    {
        ValidIssuer = _options.Issuer,
        ValidAudience = _options.Audience,
        IssuerSigningKey = JwtTokenService.CreateSigningKey(key),
        ValidateLifetime = false, // the token was issued "in 2026-11-12 09:00" by the fake clock
    };
}
