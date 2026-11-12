using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Domain.Repositories;

// Interface: a contract with no state. Callers depend on this, not on a storage technology.
public interface IEmployeeRepository
{
    Employee Add(Employee employee);
    Employee? GetById(int id);
    IReadOnlyList<Employee> GetAll();
    IReadOnlyList<Employee> GetByDepartment(int departmentId);
    bool Remove(int id);
}
