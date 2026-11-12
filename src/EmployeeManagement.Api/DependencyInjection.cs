using EmployeeManagement.Api.Configuration;
using EmployeeManagement.Api.Services;
using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Repositories;
using EmployeeManagement.Domain.Samples;
using EmployeeManagement.Domain.Services;

namespace EmployeeManagement.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddEmployeeManagement(this IServiceCollection services, IConfiguration configuration)
    {
        // Bad settings stop the app at startup instead of failing on the first request.
        services.AddOptions<RequestLoggingOptions>()
            .Bind(configuration.GetSection(RequestLoggingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<HiringOptions>()
            .Bind(configuration.GetSection(HiringOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Singleton: one instance for the whole app. Must be safe to share between concurrent requests.
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IDepartmentRepository>(_ => Seed(new InMemoryDepartmentRepository(), SampleData.Create().Departments));
        services.AddSingleton<IEmployeeRepository>(_ => Seed(new InMemoryEmployeeRepository(), SampleData.Create().Employees));

        // Scoped: one instance per HTTP request, shared by everything that handles that request.
        services.AddScoped<ICorrelationContext, CorrelationContext>();
        services.AddScoped<OrganizationService>();

        // Transient: a new instance every time it is resolved. Use for light, stateless services.
        services.AddTransient<IEmployeeFactory, EmployeeFactory>();

        return services;
    }

    private static TRepository Seed<TRepository, TEntity>(TRepository repository, IEnumerable<TEntity> items)
        where TRepository : InMemoryRepository<TEntity>
        where TEntity : Entity
    {
        foreach (TEntity item in items)
        {
            repository.Add(item);
        }

        return repository;
    }
}
