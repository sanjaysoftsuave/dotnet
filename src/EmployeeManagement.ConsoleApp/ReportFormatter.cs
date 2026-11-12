using System.Globalization;
using System.Text;
using EmployeeManagement.Domain.Employees;
using EmployeeManagement.Domain.Queries;
using EmployeeManagement.Domain.Repositories;

namespace EmployeeManagement.ConsoleApp;

public static class ReportFormatter
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    // StringBuilder avoids allocating a new string for every '+' inside the loop.
    public static string FormatTable(IReadOnlyList<Employee> employees, IDepartmentRepository departments)
    {
        if (employees.Count == 0)
        {
            return "No employees found.";
        }

        var builder = new StringBuilder();
        builder.AppendLine(string.Format(Culture, "{0,-4} {1,-20} {2,-12} {3,12} {4}", "Id", "Name", "Department", "Annual cost", "Hired"));
        builder.AppendLine(new string('-', 62));

        foreach (Employee e in employees)
        {
            string department = departments.GetById(e.DepartmentId)?.Name ?? "?";
            builder.AppendLine(string.Format(Culture, "{0,-4} {1,-20} {2,-12} {3,12:N2} {4:yyyy-MM-dd}",
                e.Id, e.FullName, department, e.CalculateAnnualCost(), e.HireDate));
        }

        builder.Append(Culture, $"{employees.Count} employee(s)");
        return builder.ToString();
    }

    public static string FormatSummary(CostStatistics stats, IEnumerable<DepartmentCostSummary> departments)
    {
        var builder = new StringBuilder();
        builder.AppendLine(Culture, $"Employees  : {stats.Count}");
        builder.AppendLine(Culture, $"Total cost : {stats.Total:N2}");
        builder.AppendLine(Culture, $"Average    : {stats.Average:N2}");
        builder.AppendLine(Culture, $"Min / Max  : {stats.Min:N2} / {stats.Max:N2}");
        builder.AppendLine("By department:");

        foreach (DepartmentCostSummary d in departments)
        {
            builder.AppendLine(Culture, $"  {d.Department,-12} {d.HeadCount} staff {d.TotalCost,14:N2}");
        }

        return builder.ToString().TrimEnd();
    }

    public static string FormatImport(LoadedOrganization org)
    {
        var builder = new StringBuilder();
        builder.Append(Culture, $"Imported {org.Import.Imported} employee(s) into {org.Departments.GetAll().Count} department(s), ");
        builder.Append(Culture, $"{org.Import.Errors.Count} row(s) skipped.");
        return builder.ToString();
    }
}
