using EmployeeManagement.Domain.Common;

namespace EmployeeManagement.Domain.Employees;

public sealed class Contractor : Employee
{
    public Contractor(
        string firstName,
        string lastName,
        string? email,
        int departmentId,
        DateOnly hireDate,
        decimal hourlyRate,
        int hoursPerYear,
        DateOnly contractEnd)
        : base(firstName, lastName, email, departmentId, hireDate)
    {
        if (contractEnd <= hireDate)
        {
            throw new ArgumentException("Contract must end after it starts.", nameof(contractEnd));
        }

        HourlyRate = Guard.NotNegative(hourlyRate, nameof(hourlyRate));
        HoursPerYear = Guard.Positive(hoursPerYear, nameof(hoursPerYear));
        ContractEnd = contractEnd;
    }

    public decimal HourlyRate { get; private set; }
    public int HoursPerYear { get; private set; }
    public DateOnly ContractEnd { get; private set; }

    public override EmploymentType EmploymentType => EmploymentType.Contractor;

    public override decimal CalculateAnnualCost() => HourlyRate * HoursPerYear;

    public bool IsActiveOn(DateOnly date) => date >= HireDate && date <= ContractEnd;

    public void Extend(DateOnly newEnd)
    {
        if (newEnd <= ContractEnd)
        {
            throw new DomainException("A contract can only be extended to a later date.");
        }

        ContractEnd = newEnd;
    }
}
