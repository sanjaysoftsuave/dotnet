using EmployeeManagement.Application.Abstractions;
using EmployeeManagement.Infrastructure.Persistence.InMemory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Singleton: the shared in-memory store. Scoped: the repositories that use it, one set per request.
        services.AddSingleton(_ => InMemoryDatabase.WithSampleData());
        services.AddScoped<IEmployeeRepository, InMemoryEmployeeRepository>();
        services.AddScoped<IDepartmentRepository, InMemoryDepartmentRepository>();
        services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();

        return services;
    }
}
