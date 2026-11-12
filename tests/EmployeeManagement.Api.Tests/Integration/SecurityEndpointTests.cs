using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmployeeManagement.Api.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace EmployeeManagement.Api.Tests.Integration;

// End-to-end through the real pipeline: authentication, authorization, controllers, EF Core and ProblemDetails.
public class SecurityEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly object NewEmployee = new
    {
        firstName = "Priya",
        lastName = "Shah",
        departmentId = 2,
        hireDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
        employmentType = "FullTime",
        annualSalary = 110_000,
    };

    [Fact]
    public async Task NoToken_Returns401ProblemWithBearerChallenge()
    {
        // Act
        HttpResponseMessage response = await factory.CreateClient().GetAsync("/api/employees");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.Single().Scheme);
        Assert.Equal(401, (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Status);
    }

    [Fact]
    public async Task Viewer_CanRead_Returns200()
    {
        HttpResponseMessage response = await factory.CreateClientWithRole(Roles.Viewer).GetAsync("/api/employees/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotCreateEmployee_Returns403()
    {
        // Act
        HttpResponseMessage response = await factory.CreateClientWithRole(Roles.Viewer).PostAsJsonAsync("/api/employees", NewEmployee);

        // Assert: authenticated but not allowed
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(403, (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Status);
    }

    [Fact]
    public async Task HrManager_CanCreateEmployee_Returns201()
    {
        HttpResponseMessage response = await factory.CreateClientWithRole(Roles.HrManager).PostAsJsonAsync("/api/employees", NewEmployee);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task HrManager_CannotCreateDepartment_ButHrAdminCan()
    {
        var department = new { name = $"Finance {Guid.NewGuid():N}"[..20], code = "FIN" + Random.Shared.Next(100, 999), annualBudget = 100_000 };

        HttpResponseMessage asManager = await factory.CreateClientWithRole(Roles.HrManager).PostAsJsonAsync("/api/departments", department);
        HttpResponseMessage asAdmin = await factory.CreateClientWithRole(Roles.HrAdmin).PostAsJsonAsync("/api/departments", department);

        Assert.Equal(HttpStatusCode.Forbidden, asManager.StatusCode);
        Assert.Equal(HttpStatusCode.Created, asAdmin.StatusCode);
    }

    [Fact]
    public async Task TokenSignedWithAnotherKey_Returns401()
    {
        // Arrange: same issuer and audience, wrong key, as a forged token would have
        var forger = new JwtTokenService(
            Options.Create(new JwtOptions { Issuer = "EmployeeManagement.Api", Audience = "EmployeeManagement.Clients", SigningKey = "a-completely-different-key-0123456789abcdef" }),
            TimeProvider.System);

        // Act
        HttpResponseMessage response = await SendWithToken(forger.CreateToken("mallory", [Roles.HrAdmin]).AccessToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExpiredToken_Returns401()
    {
        // Arrange: issued 2 hours ago with a 60-minute lifetime
        var clock = new FakeTimeProvider(DateTimeOffset.UtcNow.AddHours(-2));
        var issuer = new JwtTokenService(
            Options.Create(new JwtOptions { Issuer = "EmployeeManagement.Api", Audience = "EmployeeManagement.Clients", SigningKey = ApiFactory.SigningKey, ExpiryMinutes = 60 }),
            clock);

        // Act
        HttpResponseMessage response = await SendWithToken(issuer.CreateToken("asha", [Roles.HrAdmin]).AccessToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DemoTokenEndpoint_IssuesUsableToken()
    {
        // Arrange
        HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage tokenResponse = await client.PostAsJsonAsync("/api/auth/token", new { userName = "asha", role = Roles.Viewer });
        var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SendWithToken(token!.AccessToken)).StatusCode);
    }

    [Fact]
    public async Task DemoTokenEndpoint_UnknownRole_Returns400()
    {
        HttpResponseMessage response = await factory.CreateClient().PostAsJsonAsync("/api/auth/token", new { userName = "x", role = "SuperUser" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OpenApiDocument_IsAnonymous_AndDeclaresBearerScheme()
    {
        string document = await factory.CreateClient().GetStringAsync("/openapi/v1.json");

        Assert.Contains("\"bearer\"", document);
    }

    private Task<HttpResponseMessage> SendWithToken(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/employees/1");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return factory.CreateClient().SendAsync(request);
    }
}
