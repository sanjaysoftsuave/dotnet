namespace EmployeeManagement.Api.Services;

// Scoped: one instance per request, so every class in the request sees the same id.
public interface ICorrelationContext
{
    string CorrelationId { get; set; }
}

public sealed class CorrelationContext : ICorrelationContext
{
    public string CorrelationId { get; set; } = "";
}
