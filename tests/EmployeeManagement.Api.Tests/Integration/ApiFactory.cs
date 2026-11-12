using System.Net.Http.Headers;
using EmployeeManagement.Api.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagement.Api.Tests.Integration;

// Runs the real Program (middleware, auth, EF Core) in memory against a throwaway SQLite file.
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    // Test-only key: it signs tokens for this in-memory server and nothing else.
    public const string SigningKey = "integration-test-signing-key-0123456789abcdef";

    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"employee-api-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development"); // enables the demo token endpoint
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:EmployeeDb"] = $"Data Source={_databasePath}",
            ["Database:MigrateOnStartup"] = "true",
            ["Database:SeedSampleData"] = "true",
            ["Jwt:SigningKey"] = SigningKey,
            ["Logging:LogLevel:Default"] = "Warning",
        }));
    }

    public HttpClient CreateClientWithRole(string role)
    {
        HttpClient client = CreateClient();
        string token = Services.GetRequiredService<ITokenService>().CreateToken($"test-{role}", [role]).AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools(); // release the file handle so it can be deleted
        foreach (string file in new[] { _databasePath, _databasePath + "-wal", _databasePath + "-shm" })
        {
            File.Delete(file);
        }
    }
}
