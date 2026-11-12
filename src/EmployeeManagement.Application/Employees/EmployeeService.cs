using EmployeeManagement.Application.Abstractions;
using EmployeeManagement.Application.Common;
using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Employees;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Application.Employees;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeResponse>> ListAsync(CancellationToken cancellationToken);
    Task<EmployeeResponse> GetAsync(int id, CancellationToken cancellationToken);
    Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken);
    Task UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken);
    Task<EmployeeResponse> GiveRaiseAsync(int id, RaiseRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}

// Use-case layer: checks rules that need data, calls the domain model, then saves once.
// It knows nothing about HTTP (no status codes, no IActionResult) and nothing about the database.
public sealed class EmployeeService(
    IEmployeeRepository employees,
    IDepartmentRepository departments,
    IUnitOfWork unitOfWork,
    IEmployeeFactory factory,
    ILogger<EmployeeService> logger) : IEmployeeService
{
    public async Task<IReadOnlyList<EmployeeResponse>> ListAsync(CancellationToken cancellationToken) =>
        (await employees.ListAsync(cancellationToken)).Select(e => e.ToResponse()).ToList();

    public async Task<EmployeeResponse> GetAsync(int id, CancellationToken cancellationToken) =>
        (await GetEmployeeAsync(id, cancellationToken)).ToResponse();

    public async Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        await CheckReferencesAsync(errors, request.DepartmentId, request.ManagerId, cancellationToken);
        errors.ThrowIfAny();

        Employee employee = factory.Create(request);
        employees.Add(employee);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Hired employee {EmployeeId} into department {DepartmentId}", employee.Id, employee.DepartmentId);
        return employee.ToResponse();
    }

    public async Task UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        Employee employee = await GetEmployeeAsync(id, cancellationToken);

        // Validate everything first so a failed request never leaves the employee half-updated.
        var errors = new ValidationErrors();
        await CheckReferencesAsync(errors, request.DepartmentId, request.ManagerId, cancellationToken);
        if (request.ManagerId == id)
        {
            errors.Add(nameof(request.ManagerId), "An employee cannot manage themselves.");
        }

        errors.ThrowIfAny();

        employee.Rename(request.FirstName, request.LastName);
        employee.ChangeEmail(request.Email);
        employee.TransferTo(request.DepartmentId);
        employee.AssignManager(request.ManagerId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmployeeResponse> GiveRaiseAsync(int id, RaiseRequest request, CancellationToken cancellationToken)
    {
        if (await GetEmployeeAsync(id, cancellationToken) is not FullTimeEmployee employee)
        {
            throw new DomainException("Only full-time employees can receive a raise.");
        }

        employee.GiveRaise(request.Percent);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Gave employee {EmployeeId} a raise of {Percent}%", id, request.Percent);
        return employee.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        Employee employee = await GetEmployeeAsync(id, cancellationToken);

        if (await employees.AnyReportingToAsync(id, cancellationToken))
        {
            throw new ConflictException($"{employee.FullName} still manages other employees. Reassign them first.");
        }

        employees.Remove(employee);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Removed employee {EmployeeId}", id);
    }

    private async Task<Employee> GetEmployeeAsync(int id, CancellationToken cancellationToken) =>
        await employees.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Employee", id);

    private async Task CheckReferencesAsync(ValidationErrors errors, int departmentId, int? managerId, CancellationToken cancellationToken)
    {
        if (!await departments.ExistsAsync(departmentId, cancellationToken))
        {
            errors.Add("DepartmentId", $"Department {departmentId} does not exist.");
        }

        if (managerId is int id && !await employees.ExistsAsync(id, cancellationToken))
        {
            errors.Add("ManagerId", $"Employee {id} does not exist.");
        }
    }
}
