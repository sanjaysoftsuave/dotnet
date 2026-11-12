using EmployeeManagement.Api.Configuration;
using EmployeeManagement.Api.Contracts;
using EmployeeManagement.Api.Mapping;
using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Employees;
using Microsoft.Extensions.Options;

namespace EmployeeManagement.Api.Services;

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
            throw new DomainException($"Hire date cannot be later than {latest:yyyy-MM-dd}.");
        }

        return request.ToEmployee();
    }
}
