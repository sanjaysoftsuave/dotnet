using EmployeeManagement.Api.Configuration;
using EmployeeManagement.Api.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace EmployeeManagement.Api.Middleware;

// Logs one line per request with a correlation id. Only method, path, status and timing are logged:
// query strings, headers and bodies can carry personal data or tokens, so they are never written.
public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger,
    IOptions<RequestLoggingOptions> options,
    TimeProvider time)
{
    private const int MaxCorrelationIdLength = 64;

    // Middleware is created once (a singleton), so scoped services are taken here per request,
    // never in the constructor. Injecting them in the constructor would be a captive dependency.
    public async Task InvokeAsync(HttpContext context, ICorrelationContext correlation)
    {
        RequestLoggingOptions settings = options.Value;
        string correlationId = ReadOrCreateCorrelationId(context.Request.Headers[settings.CorrelationHeaderName]);
        correlation.CorrelationId = correlationId;
        context.Response.Headers[settings.CorrelationHeaderName] = correlationId;

        // Every log written during this request (here, in controllers, in services) carries the CorrelationId property.
        using IDisposable? scope = logger.BeginScope("CorrelationId:{CorrelationId}", correlationId);
        long started = time.GetTimestamp();
        int? failedStatus = null;

        try
        {
            await next(context);
        }
        catch
        {
            failedStatus = StatusCodes.Status500InternalServerError;
            throw;
        }
        finally
        {
            int status = failedStatus ?? context.Response.StatusCode;
            double elapsedMs = time.GetElapsedTime(started).TotalMilliseconds;
            LogLevel level = status >= 500 ? LogLevel.Error
                : elapsedMs >= settings.SlowRequestThresholdMs ? LogLevel.Warning
                : LogLevel.Information;

            logger.Log(level, "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs:0.0} ms",
                context.Request.Method, context.Request.Path.Value, status, elapsedMs);
        }
    }

    // Reuse the caller's id so logs can be joined across services, but only if it is safe to echo back.
    private static string ReadOrCreateCorrelationId(StringValues incoming)
    {
        string? value = incoming.Count == 1 ? incoming[0] : null;
        bool isSafe = value is { Length: > 0 and <= MaxCorrelationIdLength } &&
            value.All(c => char.IsAsciiLetterOrDigit(c) || c == '-');

        return isSafe ? value! : Guid.NewGuid().ToString("N");
    }
}
