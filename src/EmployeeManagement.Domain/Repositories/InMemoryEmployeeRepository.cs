using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Domain.Repositories;

public sealed class InMemoryEmployeeRepository : IEmployeeRepository
{
    private readonly Dictionary<int, Employee> _employees = new();
    private int _nextId = 1;

    public Employee Add(Employee employee)
    {
        if (employee.Id == 0)
        {
            employee.AssignId(_nextId);
        }

        if (!_employees.TryAdd(employee.Id, employee))
        {
            throw new InvalidOperationException($"Employee {employee.Id} already exists.");
        }

        _nextId = Math.Max(_nextId, employee.Id + 1);
        return employee;
    }

    public Employee? GetById(int id) => _employees.GetValueOrDefault(id);

    public IReadOnlyList<Employee> GetAll()
    {
        var all = new List<Employee>(_employees.Values);
        all.Sort((a, b) => a.Id.CompareTo(b.Id));
        return all;
    }

    public IReadOnlyList<Employee> GetByDepartment(int departmentId)
    {
        var matches = new List<Employee>();
        foreach (Employee employee in GetAll())
        {
            if (employee.DepartmentId == departmentId)
            {
                matches.Add(employee);
            }
        }

        return matches;
    }

    public bool Remove(int id) => _employees.Remove(id);
}
