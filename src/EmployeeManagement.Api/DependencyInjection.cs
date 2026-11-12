using EmployeeManagement.Api.Configuration;
using EmployeeManagement.Api.ErrorHandling;
using EmployeeManagement.Api.Services;
using EmployeeManagement.Application;
using EmployeeManagement.Infrastructure;

namespace EmployeeManagement.Api;

public static class DependencyInjection
{
    // Composition root: the only place that knows about every layer.
    public static IServiceCollection AddEmployeeManagement(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication(configuration);
        services.AddInfrastructure(configuration);

        services.AddOptions<RequestLoggingOptions>()
            .Bind(configuration.GetSection(RequestLoggingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Scoped: one correlation id per request.
        services.AddScoped<ICorrelationContext, CorrelationContext>();

        // Every error response, including automatic 400s and empty 404s, gets the same shape plus a correlation id.
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            string? correlationId = context.HttpContext.RequestServices.GetService<ICorrelationContext>()?.CorrelationId;
            if (!string.IsNullOrEmpty(correlationId))
            {
                context.ProblemDetails.Extensions["correlationId"] = correlationId;
            }
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}
