using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Domain.Repositories;

public sealed class InMemoryEmployeeRepository : InMemoryRepository<Employee>, IEmployeeRepository
{
    public IReadOnlyList<Employee> GetByDepartment(int departmentId) =>
        Items.Where(e => e.DepartmentId == departmentId).OrderBy(e => e.Id).ToList();
}
