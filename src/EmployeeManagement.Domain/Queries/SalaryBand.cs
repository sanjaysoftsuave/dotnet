using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Domain.Queries;

public enum SalaryBand
{
    Junior,
    Mid,
    Senior,
    Contract,
}

public static class SalaryBands
{
    public const decimal SeniorThreshold = 150_000m;
    public const decimal MidThreshold = 90_000m;

    // Type and property patterns in a switch expression.
    public static SalaryBand Classify(Employee employee) => employee switch
    {
        Contractor => SalaryBand.Contract,
        FullTimeEmployee { AnnualSalary: >= SeniorThreshold } => SalaryBand.Senior,
        FullTimeEmployee { AnnualSalary: >= MidThreshold } => SalaryBand.Mid,
        FullTimeEmployee => SalaryBand.Junior,
        _ => throw new ArgumentOutOfRangeException(nameof(employee), employee.GetType().Name, "Unknown employee kind."),
    };
}
