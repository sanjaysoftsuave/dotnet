using EmployeeManagement.Domain.Departments;

namespace EmployeeManagement.Domain.Repositories;

public interface IDepartmentRepository
{
    Department Add(Department department);
    Department? GetById(int id);
    Department? GetByName(string name);
    IReadOnlyList<Department> GetAll();
    bool Remove(int id);
}
