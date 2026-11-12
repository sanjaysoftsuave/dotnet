using EmployeeManagement.Domain.Departments;

namespace EmployeeManagement.Domain.Repositories;

public sealed class InMemoryDepartmentRepository : InMemoryRepository<Department>, IDepartmentRepository
{
    public override Department Add(Department department)
    {
        if (GetByName(department.Name) is not null)
        {
            throw new InvalidOperationException($"Department '{department.Name}' already exists.");
        }

        return base.Add(department);
    }

    public Department? GetByName(string name) =>
        Items.FirstOrDefault(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
}
