using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Domain.Queries;

// LINQ exercises as extension methods. Methods returning IEnumerable<T> are deferred:
// nothing runs until the caller enumerates (foreach, ToList, Count...).
public static class EmployeeQueries
{
    // Q1 Where + string search
    public static IEnumerable<Employee> SearchByName(this IEnumerable<Employee> employees, string text) =>
        employees.Where(e => e.FullName.Contains(text, StringComparison.OrdinalIgnoreCase));

    // Q2 Where on a date range, inclusive of both ends
    public static IEnumerable<Employee> HiredBetween(this IEnumerable<Employee> employees, DateOnly from, DateOnly to)
    {
        if (from > to)
        {
            throw new ArgumentException("'from' must be on or before 'to'.", nameof(from));
        }

        return employees.Where(e => e.HireDate >= from && e.HireDate <= to);
    }

    // Q3 OrderBy + ThenBy
    public static IEnumerable<Employee> OrderBySeniority(this IEnumerable<Employee> employees) =>
        employees.OrderBy(e => e.HireDate).ThenBy(e => e.LastName);

    // Q4 OrderByDescending + Take
    public static IEnumerable<Employee> TopByCost(this IEnumerable<Employee> employees, int count) =>
        employees.OrderByDescending(e => e.CalculateAnnualCost()).ThenBy(e => e.Id).Take(count);

    // Q5 GroupJoin + aggregate (departments without staff are still listed)
    public static IEnumerable<DepartmentCostSummary> CostByDepartment(
        this IEnumerable<Employee> employees, IEnumerable<Department> departments) =>
        departments
            .GroupJoin(
                employees,
                d => d.Id,
                e => e.DepartmentId,
                (d, staff) => new DepartmentCostSummary(
                    d.Name, staff.Count(), staff.Sum(e => e.CalculateAnnualCost()), d.AnnualBudget))
            .OrderByDescending(s => s.TotalCost);

    // Q6 GroupBy + ToDictionary (executes immediately)
    public static Dictionary<EmploymentType, int> CountByEmploymentType(this IEnumerable<Employee> employees) =>
        employees.GroupBy(e => e.EmploymentType).ToDictionary(g => g.Key, g => g.Count());

    // Q7 GroupBy on a computed key (pattern-matching classifier)
    public static IEnumerable<IGrouping<SalaryBand, Employee>> GroupBySalaryBand(this IEnumerable<Employee> employees) =>
        employees.GroupBy(SalaryBands.Classify).OrderBy(g => g.Key);

    // Q8 OfType + Where
    public static IEnumerable<Contractor> ActiveContractorsOn(this IEnumerable<Employee> employees, DateOnly date) =>
        employees.OfType<Contractor>().Where(c => c.IsActiveOn(date));

    // Q9 GroupBy + Join + Select into a record
    public static IEnumerable<ManagerSummary> ManagersWithReports(this IEnumerable<Employee> employees) =>
        employees
            .Where(e => e.ManagerId is not null)
            .GroupBy(e => e.ManagerId!.Value)
            .Join(employees, g => g.Key, m => m.Id, (g, m) => new ManagerSummary(m.Id, m.FullName, g.Count()))
            .OrderByDescending(s => s.DirectReports)
            .ThenBy(s => s.ManagerName);

    // Q10 Combine any number of predicate delegates
    public static IEnumerable<Employee> MatchingAll(this IEnumerable<Employee> employees, params Func<Employee, bool>[] predicates) =>
        employees.Where(e => predicates.All(predicate => predicate(e)));

    // Q11 Any (stops at the first match)
    public static bool AnyWithoutEmail(this IEnumerable<Employee> employees) =>
        employees.Any(e => e.Email is null);

    // Q12 Generic paging (1-based page numbers)
    public static IEnumerable<T> Page<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    // Q13 Aggregate: count, total, min and max in a single pass
    public static CostStatistics SummarizeCost(this IEnumerable<Employee> employees) =>
        employees.Aggregate(default(CostStatistics), (stats, e) => stats.Add(e.CalculateAnnualCost()));
}
