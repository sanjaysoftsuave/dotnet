using EmployeeManagement.Domain.Samples;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmployeeManagement.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    // Convenient for local development. In shared environments run `dotnet ef database update` instead.
    public bool MigrateOnStartup { get; set; }
    public bool SeedSampleData { get; set; }
}

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = services.CreateScope();
        DatabaseOptions options = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        var db = scope.ServiceProvider.GetRequiredService<EmployeeManagementDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<EmployeeManagementDbContext>>();

        if (options.MigrateOnStartup)
        {
            await db.Database.MigrateAsync(cancellationToken);
        }

        if (options.SeedSampleData && !await db.Departments.AnyAsync(cancellationToken))
        {
            SampleOrganization sample = SampleData.Create();
            db.Departments.AddRange(sample.Departments);
            db.Employees.AddRange(sample.Employees);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded {Departments} departments and {Employees} employees",
                sample.Departments.Count, sample.Employees.Count);
        }
    }
}
