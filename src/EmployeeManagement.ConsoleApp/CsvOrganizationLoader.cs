using EmployeeManagement.Domain.Import;
using EmployeeManagement.Domain.Repositories;
using EmployeeManagement.Domain.Services;

namespace EmployeeManagement.ConsoleApp;

public sealed record LoadedOrganization(
    InMemoryEmployeeRepository Employees,
    InMemoryDepartmentRepository Departments,
    ImportResult Import);

public static class CsvOrganizationLoader
{
    public static async Task<LoadedOrganization> LoadAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Input file '{path}' was not found.", path);
        }

        var employees = new InMemoryEmployeeRepository();
        var departments = new InMemoryDepartmentRepository();
        var importer = new EmployeeCsvImporter(new OrganizationService(employees, departments), departments);

        using StreamReader reader = File.OpenText(path);
        ImportResult result = await importer.ImportAsync(reader, cancellationToken);
        return new LoadedOrganization(employees, departments, result);
    }
}
