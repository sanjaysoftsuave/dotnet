using System.Globalization;
using System.Text;

namespace EmployeeManagement.ConsoleApp;

public static class ReportFormatter
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    // StringBuilder avoids allocating a new string for every '+' inside the loop.
    public static string FormatTable(IReadOnlyList<EmployeeRecord> employees)
    {
        if (employees.Count == 0)
        {
            return "No employees found.";
        }

        var builder = new StringBuilder();
        builder.AppendLine(string.Format(Culture, "{0,-4} {1,-20} {2,-12} {3,12} {4}", "Id", "Name", "Department", "Salary", "Hired"));
        builder.AppendLine(new string('-', 62));

        foreach (EmployeeRecord e in employees)
        {
            builder.AppendLine(string.Format(Culture, "{0,-4} {1,-20} {2,-12} {3,12:N2} {4:yyyy-MM-dd}",
                e.Id, e.FullName, e.Department, e.Salary, e.HireDate));
        }

        builder.Append(Culture, $"{employees.Count} employee(s)");
        return builder.ToString();
    }

    public static string FormatSummary(SalaryStats stats, Dictionary<string, int> departmentCounts)
    {
        var builder = new StringBuilder();
        builder.AppendLine(Culture, $"Employees : {stats.Count}");
        builder.AppendLine(Culture, $"Total     : {stats.Total:N2}");
        builder.AppendLine(Culture, $"Average   : {stats.Average:N2}");
        builder.AppendLine(Culture, $"Min / Max : {stats.Min:N2} / {stats.Max:N2}");
        builder.AppendLine("By department:");

        string[] names = new string[departmentCounts.Count];
        departmentCounts.Keys.CopyTo(names, 0);
        Array.Sort(names, StringComparer.OrdinalIgnoreCase);

        foreach (string name in names)
        {
            builder.AppendLine(Culture, $"  {name,-12} {departmentCounts[name]}");
        }

        return builder.ToString().TrimEnd();
    }
}
