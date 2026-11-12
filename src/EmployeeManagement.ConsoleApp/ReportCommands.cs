using System.Globalization;
using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;
using EmployeeManagement.Domain.Queries;

namespace EmployeeManagement.ConsoleApp;

public static class ReportCommands
{
    public static string Execute(LoadedOrganization org, string command, IReadOnlyList<string> arguments)
    {
        IReadOnlyList<Employee> staff = org.Employees.GetAll();

        return command.ToLowerInvariant() switch
        {
            "list" => ReportFormatter.FormatTable(staff, org.Departments),
            "summary" => ReportFormatter.FormatSummary(staff.SummarizeCost(), staff.CostByDepartment(org.Departments.GetAll())),
            "search" => ReportFormatter.FormatTable(staff.SearchByName(Argument(arguments, 0, "search text")).ToList(), org.Departments),
            "dept" => ReportFormatter.FormatTable(InDepartment(org, Argument(arguments, 0, "department name")), org.Departments),
            "hired" => ReportFormatter.FormatTable(
                staff.HiredBetween(ParseDate(Argument(arguments, 0, "from date")), ParseDate(Argument(arguments, 1, "to date"))).ToList(),
                org.Departments),
            "raise" => Raise(org, ParsePercent(Argument(arguments, 0, "percent"))),
            _ => throw new ArgumentException($"Unknown command '{command}'. Run without arguments for help."),
        };
    }

    private static IReadOnlyList<Employee> InDepartment(LoadedOrganization org, string name)
    {
        Department department = org.Departments.GetByName(name)
            ?? throw new ArgumentException($"Department '{name}' was not found.");
        return org.Employees.GetByDepartment(department.Id);
    }

    private static string Raise(LoadedOrganization org, decimal percent)
    {
        List<FullTimeEmployee> fullTime = org.Employees.GetAll().OfType<FullTimeEmployee>().ToList();
        decimal before = fullTime.Sum(e => e.AnnualSalary);

        foreach (FullTimeEmployee employee in fullTime)
        {
            employee.GiveRaise(percent);
        }

        decimal increase = fullTime.Sum(e => e.AnnualSalary) - before;
        return ReportFormatter.FormatTable(org.Employees.GetAll(), org.Departments) +
            string.Create(CultureInfo.InvariantCulture, $"\nTotal salary increase: {increase:N2}");
    }

    private static string Argument(IReadOnlyList<string> arguments, int index, string name) =>
        index < arguments.Count && !string.IsNullOrWhiteSpace(arguments[index])
            ? arguments[index]
            : throw new ArgumentException($"Missing argument: {name}.");

    private static DateOnly ParseDate(string value) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date)
            ? date
            : throw new ArgumentException($"'{value}' is not a valid date (yyyy-MM-dd).");

    private static decimal ParsePercent(string value) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal percent)
            ? percent
            : throw new ArgumentException($"'{value}' is not a valid percent.");
}
