using EmployeeManagement.Domain.Common;

namespace EmployeeManagement.Domain.Employees;

public sealed class FullTimeEmployee : Employee
{
    public const decimal DefaultBenefitsRate = 0.20m;

    public FullTimeEmployee(
        string firstName,
        string lastName,
        string? email,
        int departmentId,
        DateOnly hireDate,
        decimal annualSalary,
        decimal benefitsRate = DefaultBenefitsRate)
        : base(firstName, lastName, email, departmentId, hireDate)
    {
        AnnualSalary = Guard.NotNegative(annualSalary, nameof(annualSalary));
        BenefitsRate = benefitsRate is >= 0 and <= 1
            ? benefitsRate
            : throw new ArgumentOutOfRangeException(nameof(benefitsRate), benefitsRate, "Benefits rate must be between 0 and 1.");
    }

    public decimal AnnualSalary { get; private set; }
    public decimal BenefitsRate { get; }

    public override EmploymentType EmploymentType => EmploymentType.FullTime;

    public override decimal CalculateAnnualCost() => Math.Round(AnnualSalary * (1 + BenefitsRate), 2);

    public void GiveRaise(decimal percent)
    {
        if (percent is <= 0 or > 50)
        {
            throw new DomainException("A raise must be greater than 0% and at most 50%.");
        }

        AnnualSalary = Math.Round(AnnualSalary * (1 + percent / 100m), 2);
    }
}
