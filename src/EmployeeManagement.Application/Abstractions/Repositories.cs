using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Application.Abstractions;

// Persistence ports. The Application layer owns these interfaces; Infrastructure implements them.
// Add/Remove only stage a change. Nothing is stored until IUnitOfWork.SaveChangesAsync runs.

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Employee>> ListAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Employee>> ListByDepartmentAsync(int departmentId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    Task<bool> AnyInDepartmentAsync(int departmentId, CancellationToken cancellationToken);
    Task<bool> AnyReportingToAsync(int managerId, CancellationToken cancellationToken);
    void Add(Employee employee);
    void Remove(Employee employee);
}

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);
    void Add(Department department);
    void Remove(Department department);
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
