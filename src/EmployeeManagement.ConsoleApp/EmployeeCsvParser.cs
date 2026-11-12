using System.Globalization;

namespace EmployeeManagement.ConsoleApp;

public static class EmployeeCsvParser
{
    private const int ExpectedColumns = 8;

    // Expected header: Id,FirstName,LastName,Email,Department,Salary,HireDate,ManagerId
    public static List<EmployeeRecord> LoadFromFile(string path, List<string> errors)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Input file '{path}' was not found.", path);
        }

        string[] lines = File.ReadAllLines(path);
        var employees = new List<EmployeeRecord>(lines.Length);

        // Start at 1 to skip the header row.
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            try
            {
                employees.Add(ParseLine(lines[i]));
            }
            catch (FormatException ex)
            {
                errors.Add($"Line {i + 1}: {ex.Message}");
            }
        }

        return employees;
    }

    public static EmployeeRecord ParseLine(string line)
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

        return new EmployeeRecord
        {
            Id = id,
            FirstName = RequireText(parts[1], "first name"),
            LastName = RequireText(parts[2], "last name"),
            Email = NullIfEmpty(parts[3]),
            Department = RequireText(parts[4], "department"),
            Salary = salary,
            HireDate = hireDate,
            ManagerId = int.TryParse(parts[7], out int managerId) ? managerId : null,
        };
    }

    private static string RequireText(string value, string fieldName)
    {
        string trimmed = value.Trim();
        return trimmed.Length > 0 ? trimmed : throw new FormatException($"{fieldName} is required.");
    }

    private static string? NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
