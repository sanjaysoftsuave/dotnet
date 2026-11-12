using EmployeeManagement.ConsoleApp;
using EmployeeManagement.Domain.Common;

const string Usage = """
    Usage: EmployeeManagement.ConsoleApp <csv-file> <command> [arguments]
           EmployeeManagement.ConsoleApp import <csv-file>
           EmployeeManagement.ConsoleApp linq

    Commands:
      list                 Print all employees
      summary              Cost statistics and head count per department
      search <text>        Find employees by name
      dept <name>          List employees in a department
      hired <from> <to>    Employees hired between two dates (yyyy-MM-dd, inclusive)
      raise <percent>      Give every full-time employee a raise (0 < percent <= 50)
      import               Load the file and report how many rows were imported
      linq                 Run the LINQ exercises against the built-in sample data

    Ctrl+C cancels loading. Exit codes: 0 ok, 1 bad input, 2 file not found, 3 cancelled.
    """;

// Cancelled by Ctrl+C or after 30 seconds, whichever comes first.
using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // keep the process alive so we can report cleanly
    cancellation.Cancel();
};

try
{
    switch (args)
    {
        case ["linq"]:
            Console.WriteLine(LinqDemo.Run());
            return 0;

        case ["import", string importPath]:
            Console.WriteLine(ReportFormatter.FormatImport(await LoadAsync(importPath)));
            return 0;

        case [string path, string command, .. string[] arguments]:
            LoadedOrganization org = await LoadAsync(path);
            Console.WriteLine(ReportCommands.Execute(org, command, arguments));
            return 0;

        default:
            Console.WriteLine(Usage);
            return 1;
    }
}
catch (FileNotFoundException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 2;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Cancelled.");
    return 3;
}
catch (Exception ex) when (ex is ArgumentException or DomainException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

async Task<LoadedOrganization> LoadAsync(string path)
{
    LoadedOrganization org = await CsvOrganizationLoader.LoadAsync(path, cancellation.Token);
    foreach (string error in org.Import.Errors)
    {
        Console.Error.WriteLine($"Skipped {error}");
    }

    return org;
}
