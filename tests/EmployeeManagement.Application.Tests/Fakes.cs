using EmployeeManagement.Application.Abstractions;
using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Application.Tests;

// Hand-written test doubles: simple lists, plus a counter so tests can check that changes were saved.

internal sealed class FakeEmployeeRepository : IEmployeeRepository
{
    public List<Employee> Items { get; } = [];

    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(e => e.Id == id));

    public Task<IReadOnlyList<Employee>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Employee>>(Items.ToList());

    public Task<IReadOnlyList<Employee>> ListByDepartmentAsync(int departmentId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Employee>>(Items.Where(e => e.DepartmentId == departmentId).ToList());

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) => Task.FromResult(Items.Any(e => e.Id == id));

    public Task<bool> AnyInDepartmentAsync(int departmentId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(e => e.DepartmentId == departmentId));

    public Task<bool> AnyReportingToAsync(int managerId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(e => e.ManagerId == managerId));

    public void Add(Employee employee)
    {
        employee.AssignId(Items.Count == 0 ? 1 : Items.Max(e => e.Id) + 1);
        Items.Add(employee);
    }

    public void Remove(Employee employee) => Items.Remove(employee);
}

internal sealed class FakeDepartmentRepository : IDepartmentRepository
{
    public List<Department> Items { get; } = [];

    public Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(d => d.Id == id));

    public Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Department>>(Items.ToList());

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) => Task.FromResult(Items.Any(d => d.Id == id));

    public Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(d => d.Id != excludeId && string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(d => d.Code == code.ToUpperInvariant()));

    public void Add(Department department)
    {
        department.AssignId(Items.Count + 1);
        Items.Add(department);
    }

    public void Remove(Department department) => Items.Remove(department);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
