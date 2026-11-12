using EmployeeManagement.Application.Abstractions;
using EmployeeManagement.Application.Common;
using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;
using EmployeeManagement.Domain.Queries;

namespace EmployeeManagement.Application.Departments;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentResponse>> ListAsync(CancellationToken cancellationToken);
    Task<DepartmentResponse> GetAsync(int id, CancellationToken cancellationToken);
    Task<DepartmentCostResponse> GetCostAsync(int id, CancellationToken cancellationToken);
    Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken);
    Task UpdateAsync(int id, UpdateDepartmentRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}

public sealed class DepartmentService(
    IDepartmentRepository departments,
    IEmployeeRepository employees,
    IUnitOfWork unitOfWork) : IDepartmentService
{
    public async Task<IReadOnlyList<DepartmentResponse>> ListAsync(CancellationToken cancellationToken) =>
        (await departments.ListAsync(cancellationToken)).Select(d => d.ToResponse()).ToList();

    public async Task<DepartmentResponse> GetAsync(int id, CancellationToken cancellationToken) =>
        (await GetDepartmentAsync(id, cancellationToken)).ToResponse();

    public async Task<DepartmentCostResponse> GetCostAsync(int id, CancellationToken cancellationToken)
    {
        Department department = await GetDepartmentAsync(id, cancellationToken);
        IReadOnlyList<Employee> staff = await employees.ListByDepartmentAsync(id, cancellationToken);
        CostStatistics cost = staff.SummarizeCost();

        return new DepartmentCostResponse(
            id, department.Name, cost.Count, cost.Total, department.AnnualBudget, cost.Total > department.AnnualBudget);
    }

    public async Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        if (await departments.NameExistsAsync(request.Name, excludeId: null, cancellationToken))
        {
            throw new ConflictException($"A department named '{request.Name}' already exists.");
        }

        if (await departments.CodeExistsAsync(request.Code, cancellationToken))
        {
            throw new ConflictException($"Department code '{request.Code.ToUpperInvariant()}' is already in use.");
        }

        var department = new Department(request.Name, request.Code, request.AnnualBudget);
        departments.Add(department);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return department.ToResponse();
    }

    public async Task UpdateAsync(int id, UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        Department department = await GetDepartmentAsync(id, cancellationToken);

        if (await departments.NameExistsAsync(request.Name, excludeId: id, cancellationToken))
        {
            throw new ConflictException($"A department named '{request.Name}' already exists.");
        }

        department.Rename(request.Name);
        department.ChangeBudget(request.AnnualBudget);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        Department department = await GetDepartmentAsync(id, cancellationToken);

        if (await employees.AnyInDepartmentAsync(id, cancellationToken))
        {
            throw new ConflictException("Move or remove its employees before deleting the department.");
        }

        departments.Remove(department);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Department> GetDepartmentAsync(int id, CancellationToken cancellationToken) =>
        await departments.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Department", id);
}
