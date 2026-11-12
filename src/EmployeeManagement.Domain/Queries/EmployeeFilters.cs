using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Domain.Queries;

// Factory methods that return delegates. Each lambda captures its argument (a closure).
public static class EmployeeFilters
{
    public static Func<Employee, bool> InDepartment(int departmentId) => e => e.DepartmentId == departmentId;

    public static Func<Employee, bool> CostsAtLeast(decimal amount) => e => e.CalculateAnnualCost() >= amount;

    public static Func<Employee, bool> OfType(EmploymentType type) => e => e.EmploymentType == type;

    public static Func<Employee, bool> HasEmail() => e => e.Email is not null;
}
