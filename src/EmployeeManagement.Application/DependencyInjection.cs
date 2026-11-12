using EmployeeManagement.Application.Departments;
using EmployeeManagement.Application.Employees;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<HiringOptions>()
            .Bind(configuration.GetSection(HiringOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Singleton: the system clock is stateless and thread-safe.
        services.AddSingleton(TimeProvider.System);

        // Scoped: services share the request's repositories and unit of work.
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();

        // Transient: light and stateless.
        services.AddTransient<IEmployeeFactory, EmployeeFactory>();

        return services;
    }
}
