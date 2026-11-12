using EmployeeManagement.Application.Abstractions;
using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Infrastructure.Persistence.InMemory;

// Changes apply immediately, so SaveChangesAsync has nothing left to do. EF Core replaces this on Day 9.

internal sealed class InMemoryEmployeeRepository(InMemoryDatabase database) : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(database.Read((employees, _) => employees.GetById(id)));

    public Task<IReadOnlyList<Employee>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult(database.Read((employees, _) => employees.GetAll()));

    public Task<IReadOnlyList<Employee>> ListByDepartmentAsync(int departmentId, CancellationToken cancellationToken) =>
        Task.FromResult(database.Read((employees, _) => employees.GetByDepartment(departmentId)));

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(database.Read((employees, _) => employees.GetById(id) is not null));

    public Task<bool> AnyInDepartmentAsync(int departmentId, CancellationToken cancellationToken) =>
        Task.FromResult(database.Read((employees, _) => employees.GetByDepartment(departmentId).Count > 0));

    public Task<bool> AnyReportingToAsync(int managerId, CancellationToken cancellationToken) =>
        Task.FromResult(database.Read((employees, _) => employees.GetAll().Any(e => e.ManagerId == managerId)));

    public void Add(Employee employee) => database.Write((employees, _) => employees.Add(employee));

    public void Remove(Employee employee) => database.Write((employees, _) => employees.Remove(employee.Id));
}

internal sealed class InMemoryDepartmentRepository(InMemoryDatabase database) : IDepartmentRepository
{
    public Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(database.Read((_, departments) => departments.GetById(id)));

    public Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult(database.Read((_, departments) => departments.GetAll()));

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(database.Read((_, departments) => departments.GetById(id) is not null));

    public Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken) =>
        Task.FromResult(database.Read((_, departments) => departments.GetAll()
            .Any(d => d.Id != excludeId && string.Equals(d.Name, name.Trim(), StringComparison.OrdinalIgnoreCase))));

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken) =>
        Task.FromResult(database.Read((_, departments) => departments.GetAll()
            .Any(d => d.Code == code.Trim().ToUpperInvariant())));

    public void Add(Department department) => database.Write((_, departments) => departments.Add(department));

    public void Remove(Department department) => database.Write((_, departments) => departments.Remove(department.Id));
}

internal sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
