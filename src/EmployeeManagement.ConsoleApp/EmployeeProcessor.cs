namespace EmployeeManagement.ConsoleApp;

// Plain loops and collections on purpose; Day 3 rewrites these with LINQ.
public static class EmployeeProcessor
{
    public static SalaryStats CalculateStats(IReadOnlyList<EmployeeRecord> employees)
    {
        if (employees.Count == 0)
        {
            return new SalaryStats(0, 0, 0, 0);
        }

        decimal total = 0;
        decimal min = decimal.MaxValue;
        decimal max = decimal.MinValue;

        foreach (EmployeeRecord employee in employees)
        {
            total += employee.Salary;
            min = Math.Min(min, employee.Salary);
            max = Math.Max(max, employee.Salary);
        }

        return new SalaryStats(employees.Count, total, min, max);
    }

    public static Dictionary<string, int> CountByDepartment(IEnumerable<EmployeeRecord> employees)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (EmployeeRecord employee in employees)
        {
            counts.TryGetValue(employee.Department, out int current);
            counts[employee.Department] = current + 1;
        }

        return counts;
    }

    public static List<EmployeeRecord> Search(IEnumerable<EmployeeRecord> employees, string text)
    {
        var matches = new List<EmployeeRecord>();
        foreach (EmployeeRecord employee in employees)
        {
            if (employee.FullName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                (employee.Email?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                matches.Add(employee);
            }
        }

        return matches;
    }

    public static List<EmployeeRecord> InDepartment(IEnumerable<EmployeeRecord> employees, string department)
    {
        var matches = new List<EmployeeRecord>();
        foreach (EmployeeRecord employee in employees)
        {
            if (string.Equals(employee.Department, department, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(employee);
            }
        }

        return matches;
    }

    // Mutates the shared EmployeeRecord objects (reference semantics) and returns the total increase.
    public static decimal ApplyRaise(IEnumerable<EmployeeRecord> employees, decimal percent)
    {
        if (percent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percent), percent, "Raise must be between 0 and 100 percent.");
        }

        decimal totalIncrease = 0;
        foreach (EmployeeRecord employee in employees)
        {
            decimal newSalary = CalculateRaisedSalary(employee.Salary, percent);
            totalIncrease += newSalary - employee.Salary;
            employee.Salary = newSalary;
        }

        return totalIncrease;
    }

    // decimal is a value type: 'salary' is a copy, so the caller's variable is untouched.
    public static decimal CalculateRaisedSalary(decimal salary, decimal percent) =>
        Math.Round(salary * (1 + percent / 100m), 2);
}
