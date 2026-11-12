using System.Globalization;

namespace EmployeeManagement.Domain.Import;

// One parsed line of: Id,FirstName,LastName,Email,Department,Salary,HireDate,ManagerId
public sealed record EmployeeCsvRow(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string Department,
    decimal Salary,
    DateOnly HireDate,
    int? ManagerId)
{
    public const string Header = "Id,FirstName,LastName,Email,Department,Salary,HireDate,ManagerId";
    private const int ExpectedColumns = 8;

    public static EmployeeCsvRow Parse(string line)
    {
        string[] parts = line.Split(',');
        if (parts.Length != ExpectedColumns)
        {
            throw new FormatException($"expected {ExpectedColumns} columns but found {parts.Length}.");
        }

        if (!int.TryParse(parts[0], out int id) || id <= 0)
        {
            throw new FormatException($"'{parts[0]}' is not a valid positive id.");
        }

        if (!decimal.TryParse(parts[5], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal salary) || salary < 0)
        {
            throw new FormatException($"'{parts[5]}' is not a valid salary.");
        }

        if (!DateOnly.TryParseExact(parts[6], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly hireDate))
        {
            throw new FormatException($"'{parts[6]}' is not a valid hire date (yyyy-MM-dd).");
        }

        int? managerId = null;
        if (!string.IsNullOrWhiteSpace(parts[7]))
        {
            managerId = int.TryParse(parts[7], out int parsed) && parsed > 0
                ? parsed
                : throw new FormatException($"'{parts[7]}' is not a valid manager id.");
        }

        return new EmployeeCsvRow(
            id,
            parts[1].Trim(),
            parts[2].Trim(),
            string.IsNullOrWhiteSpace(parts[3]) ? null : parts[3].Trim(),
            parts[4].Trim(),
            salary,
            hireDate,
            managerId);
    }
}
