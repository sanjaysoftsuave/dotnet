using System.Globalization;
using System.Text;
using EmployeeManagement.Domain.Employees;
using EmployeeManagement.Domain.Queries;
using EmployeeManagement.Domain.Samples;

namespace EmployeeManagement.ConsoleApp;

// Runs every query in EmployeeQueries against the sample organisation.
public static class LinqDemo
{
    private static readonly DateOnly AsOf = new(2026, 1, 1);

    public static string Run()
    {
        SampleOrganization org = SampleData.Create();
        IReadOnlyList<Employee> staff = org.Employees;
        var output = new StringBuilder();

        Section(output, "Q1 SearchByName(\"an\")", Names(staff.SearchByName("an")));
        Section(output, "Q2 HiredBetween(2019-01-01, 2021-12-31)", Names(staff.HiredBetween(new(2019, 1, 1), new(2021, 12, 31))));
        Section(output, "Q3 OrderBySeniority() first 3", Names(staff.OrderBySeniority().Take(3)));
        Section(output, "Q4 TopByCost(3)", staff.TopByCost(3).Select(e => $"{e.FullName} {Money(e.CalculateAnnualCost())}"));
        Section(output, "Q5 CostByDepartment()", staff.CostByDepartment(org.Departments)
            .Select(s => $"{s.Department}: {s.HeadCount} staff, {Money(s.TotalCost)} of {Money(s.Budget)}{(s.IsOverBudget ? " OVER BUDGET" : "")}"));
        Section(output, "Q6 CountByEmploymentType()", staff.CountByEmploymentType().Select(kv => $"{kv.Key}: {kv.Value}"));
        Section(output, "Q7 GroupBySalaryBand()", staff.GroupBySalaryBand()
            .Select(g => $"{g.Key}: {string.Join(", ", g.Select(e => e.FirstName))}"));
        Section(output, $"Q8 ActiveContractorsOn({AsOf:yyyy-MM-dd})", Names(staff.ActiveContractorsOn(AsOf)));
        Section(output, "Q9 ManagersWithReports()", staff.ManagersWithReports().Select(m => $"{m.ManagerName}: {m.DirectReports}"));
        Section(output, "Q10 MatchingAll(InDepartment(1), CostsAtLeast(150000))",
            Names(staff.MatchingAll(EmployeeFilters.InDepartment(1), EmployeeFilters.CostsAtLeast(150_000m))));
        Section(output, "Q11 AnyWithoutEmail()", [staff.AnyWithoutEmail().ToString()]);
        Section(output, "Q12 Page(2, 4)", Names(staff.Page(2, 4)));

        return output.ToString().TrimEnd();
    }

    private static IEnumerable<string> Names(IEnumerable<Employee> employees) => employees.Select(e => e.FullName);

    private static string Money(decimal amount) => amount.ToString("N0", CultureInfo.InvariantCulture);

    private static void Section(StringBuilder output, string title, IEnumerable<string> lines)
    {
        output.AppendLine(title);
        foreach (string line in lines)
        {
            output.AppendLine($"  {line}");
        }

        output.AppendLine();
    }
}
