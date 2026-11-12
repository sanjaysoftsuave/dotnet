using EmployeeManagement.Api.Configuration;
using EmployeeManagement.Api.Middleware;
using EmployeeManagement.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace EmployeeManagement.Api.Tests;

public class RequestLoggingMiddlewareTests
{
    private const string Header = "X-Correlation-Id";
    private readonly FakeLogger<RequestLoggingMiddleware> _logger = new();
    private readonly FakeTimeProvider _time = new();
    private readonly CorrelationContext _correlation = new();

    [Fact]
    public async Task InvokeAsync_NoIncomingId_GeneratesOneAndReturnsItInResponse()
    {
        // Arrange
        var context = NewContext();
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context, _correlation);

        // Assert
        string returned = context.Response.Headers[Header].ToString();
        Assert.Equal(32, returned.Length);
        Assert.Equal(returned, _correlation.CorrelationId);
    }

    [Fact]
    public async Task InvokeAsync_SafeIncomingId_IsReused()
    {
        // Arrange
        var context = NewContext();
        context.Request.Headers[Header] = "abc-123";

        // Act
        await CreateMiddleware(_ => Task.CompletedTask).InvokeAsync(context, _correlation);

        // Assert
        Assert.Equal("abc-123", context.Response.Headers[Header].ToString());
    }

    [Theory]
    [InlineData("has spaces")]
    [InlineData("<script>")]
    public async Task InvokeAsync_UnsafeIncomingId_IsReplaced(string incoming)
    {
        // Arrange
        var context = NewContext();
        context.Request.Headers[Header] = incoming;

        // Act
        await CreateMiddleware(_ => Task.CompletedTask).InvokeAsync(context, _correlation);

        // Assert
        Assert.NotEqual(incoming, context.Response.Headers[Header].ToString());
    }

    [Fact]
    public async Task InvokeAsync_LogsMethodPathAndStatus_ButNotQueryString()
    {
        // Arrange
        var context = NewContext(query: "?email=private@example.com");
        var middleware = CreateMiddleware(ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context, _correlation);

        // Assert
        FakeLogRecord record = _logger.LatestRecord;
        Assert.Equal(LogLevel.Information, record.Level);
        Assert.Contains("GET /api/employees responded 404", record.Message);
        Assert.DoesNotContain("private@example.com", record.Message);
    }

    [Fact]
    public async Task InvokeAsync_SlowRequest_LogsWarning()
    {
        // Arrange: the fake clock moves 750 ms while the next middleware "runs"
        var middleware = CreateMiddleware(_ =>
        {
            _time.Advance(TimeSpan.FromMilliseconds(750));
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(NewContext(), _correlation);

        // Assert
        Assert.Equal(LogLevel.Warning, _logger.LatestRecord.Level);
    }

    [Fact]
    public async Task InvokeAsync_DownstreamThrows_LogsErrorWith500AndRethrows()
    {
        // Arrange
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("boom"));

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(NewContext(), _correlation));

        // Assert
        Assert.Equal(LogLevel.Error, _logger.LatestRecord.Level);
        Assert.Contains("responded 500", _logger.LatestRecord.Message);
    }

    private RequestLoggingMiddleware CreateMiddleware(RequestDelegate next) =>
        new(next, _logger, Options.Create(new RequestLoggingOptions { SlowRequestThresholdMs = 500, CorrelationHeaderName = Header }), _time);

    private static DefaultHttpContext NewContext(string query = "")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/employees";
        context.Request.QueryString = new QueryString(query);
        return context;
    }
}
