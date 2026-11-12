using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Application.Common;
using EmployeeManagement.Domain.Employees;
using Microsoft.Extensions.Options;

namespace EmployeeManagement.Application.Employees;

public sealed class HiringOptions
{
    public const string SectionName = "Hiring";

    [Range(0, 365)]
    public int MaxDaysInFuture { get; set; } = 90;
}

public interface IEmployeeFactory
{
    Employee Create(CreateEmployeeRequest request);
}

// Transient and stateless. The clock is injected so tests can control "today".
public sealed class EmployeeFactory(TimeProvider time, IOptions<HiringOptions> options) : IEmployeeFactory
{
    public Employee Create(CreateEmployeeRequest request)
    {
        DateOnly today = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        DateOnly latest = today.AddDays(options.Value.MaxDaysInFuture);

        if (request.HireDate > latest)
        {
            throw new RequestValidationException(nameof(request.HireDate), $"Hire date cannot be later than {latest:yyyy-MM-dd}.");
        }

        return request.ToEmployee();
    }
}
