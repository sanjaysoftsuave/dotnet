using EmployeeManagement.Domain.Departments;

namespace EmployeeManagement.Domain.Repositories;

public sealed class InMemoryDepartmentRepository : IDepartmentRepository
{
    private readonly Dictionary<int, Department> _departments = new();
    private int _nextId = 1;

    public Department Add(Department department)
    {
        if (GetByName(department.Name) is not null)
        {
            throw new InvalidOperationException($"Department '{department.Name}' already exists.");
        }

        if (department.Id == 0)
        {
            department.AssignId(_nextId);
        }

        if (!_departments.TryAdd(department.Id, department))
        {
            throw new InvalidOperationException($"Department {department.Id} already exists.");
        }

        _nextId = Math.Max(_nextId, department.Id + 1);
        return department;
    }

    public Department? GetById(int id) => _departments.GetValueOrDefault(id);

    public Department? GetByName(string name)
    {
        foreach (Department department in _departments.Values)
        {
            if (string.Equals(department.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return department;
            }
        }

        return null;
    }

    public IReadOnlyList<Department> GetAll()
    {
        var all = new List<Department>(_departments.Values);
        all.Sort((a, b) => a.Id.CompareTo(b.Id));
        return all;
    }

    public bool Remove(int id) => _departments.Remove(id);
}
