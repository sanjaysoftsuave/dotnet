using EmployeeManagement.Domain.Samples;
using DomainRepositories = EmployeeManagement.Domain.Repositories;

namespace EmployeeManagement.Infrastructure.Persistence.InMemory;

// Shared store for the whole app (singleton). Reuses the Week 1 in-memory repositories behind one lock,
// so concurrent requests cannot corrupt the dictionaries.
public sealed class InMemoryDatabase
{
    private readonly Lock _gate = new();
    private readonly DomainRepositories.InMemoryEmployeeRepository _employees = new();
    private readonly DomainRepositories.InMemoryDepartmentRepository _departments = new();

    public static InMemoryDatabase WithSampleData()
    {
        var database = new InMemoryDatabase();
        SampleOrganization sample = SampleData.Create();

        foreach (var department in sample.Departments)
        {
            database._departments.Add(department);
        }

        foreach (var employee in sample.Employees)
        {
            database._employees.Add(employee);
        }

        return database;
    }

    public T Read<T>(Func<DomainRepositories.IEmployeeRepository, DomainRepositories.IDepartmentRepository, T> query)
    {
        lock (_gate)
        {
            return query(_employees, _departments);
        }
    }

    public void Write(Action<DomainRepositories.IEmployeeRepository, DomainRepositories.IDepartmentRepository> change)
    {
        lock (_gate)
        {
            change(_employees, _departments);
        }
    }
}
