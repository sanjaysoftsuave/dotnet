using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Domain.Samples;

public sealed record SampleOrganization(IReadOnlyList<Department> Departments, IReadOnlyList<Employee> Employees);

// Deterministic data set used by the LINQ demo, the docs and the tests.
public static class SampleData
{
    public static SampleOrganization Create()
    {
        Department engineering = WithId(new Department("Engineering", "ENG", 500_000m), 1);
        Department sales = WithId(new Department("Sales", "SAL", 250_000m), 2);
        Department hr = WithId(new Department("HR", "HR", 120_000m), 3);
        Department support = WithId(new Department("Support", "SUP", 150_000m), 4);

        Employee[] employees =
        [
            FullTime(1, "Asha", "Raman", "asha.raman@example.com", engineering, "2018-04-02", 185_000m),
            FullTime(2, "Vikram", "Iyer", "vikram.iyer@example.com", engineering, "2020-07-15", 142_000m, managerId: 1),
            FullTime(3, "Meera", "Nair", null, engineering, "2023-01-09", 98_000m, managerId: 2),
            FullTime(4, "Rahul", "Menon", "rahul.menon@example.com", sales, "2021-11-01", 76_000m, managerId: 6),
            FullTime(5, "Divya", "Krishnan", "divya.k@example.com", hr, "2019-03-18", 88_000m),
            FullTime(6, "Arjun", "Pillai", "arjun.pillai@example.com", sales, "2017-09-25", 121_000m),
            FullTime(7, "Kavya", "Suresh", "kavya.suresh@example.com", support, "2024-02-12", 64_000m, managerId: 8),
            FullTime(8, "Nikhil", "Das", "nikhil.das@example.com", support, "2016-06-30", 90_000m),
            Contract(9, "Farhan", "Ali", "farhan.ali@example.com", engineering, "2025-01-06", 75m, 1_600, "2026-12-31", managerId: 1),
            Contract(10, "Lena", "Joseph", null, support, "2024-06-03", 40m, 1_200, "2025-06-30", managerId: 8),
        ];

        return new SampleOrganization([engineering, sales, hr, support], employees);
    }

    private static FullTimeEmployee FullTime(
        int id, string first, string last, string? email, Department department, string hired, decimal salary, int? managerId = null)
    {
        var employee = new FullTimeEmployee(first, last, email, department.Id, DateOnly.Parse(hired), salary);
        return WithManager(WithId(employee, id), managerId);
    }

    private static Contractor Contract(
        int id, string first, string last, string? email, Department department, string hired,
        decimal rate, int hours, string ends, int? managerId = null)
    {
        var contractor = new Contractor(first, last, email, department.Id, DateOnly.Parse(hired), rate, hours, DateOnly.Parse(ends));
        return WithManager(WithId(contractor, id), managerId);
    }

    private static T WithId<T>(T entity, int id) where T : Common.Entity
    {
        entity.AssignId(id);
        return entity;
    }

    private static T WithManager<T>(T employee, int? managerId) where T : Employee
    {
        employee.AssignManager(managerId);
        return employee;
    }
}
