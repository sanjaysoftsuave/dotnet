using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Application.Employees;

// Filtering and sorting rules written once against IQueryable. EF Core translates them to SQL;
// unit tests run the same rules over an in-memory list with AsQueryable().
// Only use operators both can handle: no C# method calls such as CalculateAnnualCost().
public static class EmployeeQueryFilters
{
    public static IQueryable<Employee> ApplyFilters(this IQueryable<Employee> employees, EmployeeListQuery query)
    {
        if (query.DepartmentId is int departmentId)
        {
            employees = employees.Where(e => e.DepartmentId == departmentId);
        }

        employees = query.EmploymentType switch
        {
            EmploymentType.FullTime => employees.Where(e => e is FullTimeEmployee),
            EmploymentType.Contractor => employees.Where(e => e is Contractor),
            _ => employees,
        };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim().ToLower();
            employees = employees.Where(e =>
                (e.FirstName + " " + e.LastName).ToLower().Contains(term) ||
                (e.Email != null && e.Email.ToLower().Contains(term)));
        }

        if (query.HiredFrom is DateOnly from)
        {
            employees = employees.Where(e => e.HireDate >= from);
        }

        if (query.HiredTo is DateOnly to)
        {
            employees = employees.Where(e => e.HireDate <= to);
        }

        return employees;
    }

    // Always ends with Id so paging is stable when the main key has ties.
    public static IQueryable<Employee> ApplySorting(this IQueryable<Employee> employees, EmployeeListQuery query) =>
        (query.SortBy, query.SortDirection) switch
        {
            (EmployeeSortBy.LastName, SortDirection.Asc) => employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ThenBy(e => e.Id),
            (EmployeeSortBy.LastName, SortDirection.Desc) => employees.OrderByDescending(e => e.LastName).ThenByDescending(e => e.FirstName).ThenBy(e => e.Id),
            (EmployeeSortBy.HireDate, SortDirection.Asc) => employees.OrderBy(e => e.HireDate).ThenBy(e => e.Id),
            (EmployeeSortBy.HireDate, SortDirection.Desc) => employees.OrderByDescending(e => e.HireDate).ThenBy(e => e.Id),
            (_, SortDirection.Desc) => employees.OrderByDescending(e => e.Id),
            _ => employees.OrderBy(e => e.Id),
        };

    public static IQueryable<Employee> ApplyPaging(this IQueryable<Employee> employees, EmployeeListQuery query) =>
        employees.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);
}
