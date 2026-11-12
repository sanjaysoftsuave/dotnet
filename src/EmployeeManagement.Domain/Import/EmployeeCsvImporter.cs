using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;
using EmployeeManagement.Domain.Repositories;
using EmployeeManagement.Domain.Services;

namespace EmployeeManagement.Domain.Import;

public sealed record ImportResult(int Imported, IReadOnlyList<string> Errors);

public sealed class EmployeeCsvImporter(OrganizationService organization, IDepartmentRepository departments)
{
    // Reads rows asynchronously and stops with OperationCanceledException when the token is cancelled.
    // Bad rows are reported in the result instead of failing the whole import.
    public async Task<ImportResult> ImportAsync(TextReader reader, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var hired = new List<(Employee Employee, int? ManagerId, int LineNumber)>();

        string? header = await reader.ReadLineAsync(cancellationToken);
        if (header is null)
        {
            return new ImportResult(0, errors);
        }

        int lineNumber = 1;
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            lineNumber++;
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                EmployeeCsvRow row = EmployeeCsvRow.Parse(line);
                hired.Add((organization.Hire(ToEmployee(row)), row.ManagerId, lineNumber));
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException or DomainException or InvalidOperationException)
            {
                errors.Add($"Line {lineNumber}: {ex.Message}");
            }
        }

        // Managers can appear later in the file, so link them once everyone is hired.
        foreach ((Employee employee, int? managerId, int line) in hired)
        {
            try
            {
                organization.AssignManager(employee.Id, managerId);
            }
            catch (DomainException ex)
            {
                errors.Add($"Line {line}: {ex.Message}");
            }
        }

        return new ImportResult(hired.Count, errors);
    }

    private FullTimeEmployee ToEmployee(EmployeeCsvRow row)
    {
        Department department = departments.GetByName(row.Department) ?? departments.Add(CreateDepartment(row.Department));

        var employee = new FullTimeEmployee(row.FirstName, row.LastName, row.Email, department.Id, row.HireDate, row.Salary);
        employee.AssignId(row.Id);
        return employee;
    }

    // Departments found only in the file start with no budget until someone sets one.
    private static Department CreateDepartment(string name)
    {
        string letters = new(name.Where(char.IsAsciiLetterOrDigit).Take(3).ToArray());
        return new Department(name, letters.Length > 0 ? letters : "DEP", annualBudget: 0);
    }
}
