using EmployeeManagement.Application.Abstractions;
using EmployeeManagement.Infrastructure.Persistence;
using EmployeeManagement.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EmployeeManagement.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "EmployeeDb";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>().Bind(configuration.GetSection(DatabaseOptions.SectionName));

        // Scoped by default: one DbContext per request. Never make it a singleton; it is not thread-safe.
        services.AddDbContext<EmployeeManagementDbContext>((provider, options) =>
        {
            string connectionString = configuration.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is missing.");
            string root = provider.GetService<IHostEnvironment>()?.ContentRootPath ?? AppContext.BaseDirectory;
            options.UseSqlite(ResolveRelativePath(connectionString, root));
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<EmployeeManagementDbContext>());
        services.AddScoped<IEmployeeRepository, EfEmployeeRepository>();
        services.AddScoped<IDepartmentRepository, EfDepartmentRepository>();

        return services;
    }

    // "Data Source=employees.db" lands next to the API project, whatever folder `dotnet run` was started from.
    private static string ResolveRelativePath(string connectionString, string root)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (builder.DataSource is { Length: > 0 } path && path != ":memory:" && !Path.IsPathRooted(path) && builder.Mode != SqliteOpenMode.Memory)
        {
            builder.DataSource = Path.Combine(root, path);
        }

        return builder.ToString();
    }
}
