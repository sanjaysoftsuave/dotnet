using EmployeeManagement.Application.Common;
using EmployeeManagement.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.ErrorHandling;

// One place that turns exceptions into HTTP responses, so controllers and services stay free of try/catch
// and every error has the same ProblemDetails shape.
public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem = ToProblemDetails(exception);

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            logger.LogInformation("Request rejected with {StatusCode}: {Reason}", problem.Status, exception.Message);
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    public static ProblemDetails ToProblemDetails(Exception exception) => exception switch
    {
        RequestValidationException validation => new ValidationProblemDetails(validation.Errors.ToDictionary())
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
        },
        NotFoundException => Problem(StatusCodes.Status404NotFound, "Resource not found.", exception.Message),
        ConflictException => Problem(StatusCodes.Status409Conflict, "The request conflicts with the current state.", exception.Message),
        DomainException or ArgumentException => Problem(StatusCodes.Status400BadRequest, "The request breaks a business rule.", exception.Message),

        // Never echo unexpected exception messages: they can leak internals such as SQL or file paths.
        _ => Problem(StatusCodes.Status500InternalServerError, "An unexpected error occurred.", detail: null),
    };

    private static ProblemDetails Problem(int status, string title, string? detail) =>
        new() { Status = status, Title = title, Detail = detail };
}
