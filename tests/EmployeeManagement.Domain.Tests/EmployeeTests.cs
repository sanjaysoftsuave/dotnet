using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Domain.Tests;

public class EmployeeTests
{
    private static readonly DateOnly Hired = new(2020, 1, 15);

    [Fact]
    public void CalculateAnnualCost_FullTime_AddsBenefitsToSalary()
    {
        // Arrange
        var employee = new FullTimeEmployee("Asha", "Raman", null, departmentId: 1, Hired, annualSalary: 100_000m, benefitsRate: 0.25m);

        // Act
        decimal cost = employee.CalculateAnnualCost();

        // Assert
        Assert.Equal(125_000m, cost);
    }

    [Fact]
    public void CalculateAnnualCost_Contractor_IsRateTimesHours()
    {
        // Arrange
        var contractor = new Contractor("Farhan", "Ali", null, 1, Hired, hourlyRate: 50m, hoursPerYear: 1_000, Hired.AddYears(1));

        // Act
        decimal cost = contractor.CalculateAnnualCost();

        // Assert
        Assert.Equal(50_000m, cost);
    }

    [Fact]
    public void GiveRaise_ValidPercent_IncreasesSalary()
    {
        // Arrange
        var employee = new FullTimeEmployee("Asha", "Raman", null, 1, Hired, 80_000m);

        // Act
        employee.GiveRaise(10m);

        // Assert
        Assert.Equal(88_000m, employee.AnnualSalary);
    }

    [Fact]
    public void GiveRaise_ZeroPercent_ThrowsDomainException()
    {
        // Arrange
        var employee = new FullTimeEmployee("Asha", "Raman", null, 1, Hired, 80_000m);

        // Act
        void Act() => employee.GiveRaise(0m);

        // Assert
        Assert.Throws<DomainException>(Act);
        Assert.Equal(80_000m, employee.AnnualSalary);
    }

    [Fact]
    public void AssignManager_Self_ThrowsDomainException()
    {
        // Arrange
        var employee = new FullTimeEmployee("Asha", "Raman", null, 1, Hired, 80_000m);
        employee.AssignId(7);

        // Act & Assert
        Assert.Throws<DomainException>(() => employee.AssignManager(7));
    }

    [Fact]
    public void YearsOfService_DayBeforeAnniversary_DoesNotCountTheYear()
    {
        // Arrange
        var employee = new FullTimeEmployee("Asha", "Raman", null, 1, Hired, 80_000m);

        // Act
        int years = employee.YearsOfService(new DateOnly(2023, 1, 14));

        // Assert
        Assert.Equal(2, years);
    }
}
