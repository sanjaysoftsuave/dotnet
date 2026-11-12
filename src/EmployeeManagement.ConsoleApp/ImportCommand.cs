using System.Globalization;
using EmployeeManagement.Domain.Import;
using EmployeeManagement.Domain.Queries;
using EmployeeManagement.Domain.Repositories;
using EmployeeManagement.Domain.Services;

namespace EmployeeManagement.ConsoleApp;

public static class ImportCommand
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    // Cancelled by Ctrl+C or after the timeout, whichever comes first.
    public static async Task<int> RunAsync(string path)
    {
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"Input file '{path}' was not found.");
            return 2;
        }

        using var cancellation = new CancellationTokenSource(Timeout);
        ConsoleCancelEventHandler onCancel = (_, e) =>
        {
            e.Cancel = true; // keep the process alive so we can report cleanly
            cancellation.Cancel();
        };
        Console.CancelKeyPress += onCancel;

        var employees = new InMemoryEmployeeRepository();
        var departments = new InMemoryDepartmentRepository();
        var importer = new EmployeeCsvImporter(new OrganizationService(employees, departments), departments);

        try
        {
            using StreamReader reader = File.OpenText(path);
            ImportResult result = await importer.ImportAsync(reader, cancellation.Token);

            foreach (string error in result.Errors)
            {
                Console.Error.WriteLine($"Skipped {error}");
            }

            Console.WriteLine($"Imported {result.Imported} employee(s) into {departments.GetAll().Count} department(s).");
            foreach (DepartmentCostSummary summary in employees.GetAll().CostByDepartment(departments.GetAll()))
            {
                Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
                    $"  {summary.Department,-12} {summary.HeadCount} staff, annual cost {summary.TotalCost:N0}"));
            }

            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Import cancelled.");
            return 3;
        }
        finally
        {
            Console.CancelKeyPress -= onCancel;
        }
    }
}
