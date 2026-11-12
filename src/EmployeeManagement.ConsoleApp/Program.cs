using System.Globalization;
using EmployeeManagement.ConsoleApp;

const string Usage = """
    Usage: EmployeeManagement.ConsoleApp <csv-file> <command> [argument]
           EmployeeManagement.ConsoleApp linq
           EmployeeManagement.ConsoleApp import <csv-file>

    Commands:
      list                 Print all employees
      summary              Salary statistics and head count per department
      search <text>        Find employees by name or email
      dept <name>          List employees in a department
      raise <percent>      Apply a raise to everyone and print the new table
      linq                 Run the LINQ exercises against the built-in sample data
    """;

if (args is ["linq"])
{
    Console.WriteLine(LinqDemo.Run());
    return 0;
}

if (args is ["import", string importPath])
{
    return await ImportCommand.RunAsync(importPath);
}

if (args.Length < 2)
{
    Console.WriteLine(Usage);
    return 1;
}

var errors = new List<string>();
List<EmployeeRecord> employees;

try
{
    employees = EmployeeCsvParser.LoadFromFile(args[0], errors);
}
catch (FileNotFoundException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 2;
}

foreach (string error in errors)
{
    Console.Error.WriteLine($"Skipped {error}");
}

string command = args[1].ToLowerInvariant();
string? argument = args.Length > 2 ? args[2] : null;

try
{
    string output = command switch
    {
        "list" => ReportFormatter.FormatTable(employees),
        "summary" => ReportFormatter.FormatSummary(
            EmployeeProcessor.CalculateStats(employees),
            EmployeeProcessor.CountByDepartment(employees)),
        "search" => ReportFormatter.FormatTable(EmployeeProcessor.Search(employees, Require(argument, "search text"))),
        "dept" => ReportFormatter.FormatTable(EmployeeProcessor.InDepartment(employees, Require(argument, "department name"))),
        "raise" => FormatRaise(employees, ParsePercent(Require(argument, "percent"))),
        _ => throw new ArgumentException($"Unknown command '{command}'.\n\n{Usage}"),
    };

    Console.WriteLine(output);
    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static string Require(string? value, string name) =>
    string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"Missing argument: {name}.") : value;

static decimal ParsePercent(string value) =>
    decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal percent)
        ? percent
        : throw new ArgumentException($"'{value}' is not a valid percent.");

static string FormatRaise(List<EmployeeRecord> employees, decimal percent)
{
    decimal increase = EmployeeProcessor.ApplyRaise(employees, percent);
    return ReportFormatter.FormatTable(employees) +
        string.Create(CultureInfo.InvariantCulture, $"\nTotal payroll increase: {increase:N2}");
}
