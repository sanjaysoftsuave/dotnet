using System.Security.Claims;
using EmployeeManagement.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagement.Api.Tests.Security;

// The role → capability rules, checked without HTTP.
public sealed class AuthorizationPolicyTests : IDisposable
{
    private readonly ServiceProvider _provider = new ServiceCollection()
        .AddLogging()
        .AddAuthorization(Policies.Configure)
        .BuildServiceProvider();

    [Theory]
    [InlineData(Roles.Viewer, Policies.CanReadData, true)]
    [InlineData(Roles.Viewer, Policies.CanManageEmployees, false)]
    [InlineData(Roles.Viewer, Policies.CanManageDepartments, false)]
    [InlineData(Roles.HrManager, Policies.CanReadData, true)]
    [InlineData(Roles.HrManager, Policies.CanManageEmployees, true)]
    [InlineData(Roles.HrManager, Policies.CanManageDepartments, false)]
    [InlineData(Roles.HrAdmin, Policies.CanReadData, true)]
    [InlineData(Roles.HrAdmin, Policies.CanManageEmployees, true)]
    [InlineData(Roles.HrAdmin, Policies.CanManageDepartments, true)]
    public async Task Policy_ForRole_GrantsExpectedAccess(string role, string policy, bool expected)
    {
        // Act
        AuthorizationResult result = await Authorize(User(role), policy);

        // Assert
        Assert.Equal(expected, result.Succeeded);
    }

    [Fact]
    public async Task Policy_AuthenticatedUserWithUnknownRole_IsDenied()
    {
        Assert.False((await Authorize(User("Intern"), Policies.CanReadData)).Succeeded);
    }

    [Fact]
    public async Task FallbackPolicy_AnonymousUser_IsDenied()
    {
        var options = _provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthorizationOptions>>().Value;

        AuthorizationResult result = await _provider.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(new ClaimsPrincipal(new ClaimsIdentity()), resource: null, options.FallbackPolicy!);

        Assert.False(result.Succeeded);
    }

    private Task<AuthorizationResult> Authorize(ClaimsPrincipal user, string policy) =>
        _provider.GetRequiredService<IAuthorizationService>().AuthorizeAsync(user, resource: null, policy);

    private static ClaimsPrincipal User(string role) =>
        new(new ClaimsIdentity([new Claim(JwtTokenService.NameClaim, "test"), new Claim(JwtTokenService.RoleClaim, role)],
            authenticationType: "Test", JwtTokenService.NameClaim, JwtTokenService.RoleClaim));

    public void Dispose() => _provider.Dispose();
}
