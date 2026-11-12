using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Api.Configuration;

// Options pattern: strongly typed settings bound from configuration and validated at startup.
public sealed class RequestLoggingOptions
{
    public const string SectionName = "RequestLogging";

    [Range(1, 60_000)]
    public int SlowRequestThresholdMs { get; set; } = 500;

    [Required, RegularExpression("^[A-Za-z0-9-]+$")]
    public string CorrelationHeaderName { get; set; } = "X-Correlation-Id";
}
