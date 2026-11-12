using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;
using EmployeeManagement.Domain.Queries;
using EmployeeManagement.Domain.Samples;

namespace EmployeeManagement.Domain.Tests;

public class BoundaryTests
{
    private readonly SampleOrganization _org = SampleData.Create();

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, 5)]
    public void Page_NonPositiveArguments_Throws(int pageNumber, int pageSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _org.Employees.Page(pageNumber, pageSize));
    }

    [Fact]
    public void Page_LastPartialPage_ReturnsRemainingItems()
    {
        // 10 employees, pages of 4: page 3 has 2 items
        Assert.Equal([9, 10], _org.Employees.Page(3, 4).Select(e => e.Id));
    }

    [Fact]
    public void Page_BeyondTheEnd_ReturnsEmpty()
    {
        Assert.Empty(_org.Employees.Page(4, 4));
    }

    [Fact]
    public void TopByCost_MoreThanAvailable_ReturnsEveryone()
    {
        Assert.Equal(_org.Employees.Count, _org.Employees.TopByCost(100).Count());
    }

    [Fact]
    public void SummarizeCost_NoEmployees_ReturnsZeros()
    {
        // Act
        CostStatistics stats = Array.Empty<Employee>().SummarizeCost();

        // Assert
        Assert.Equal(default, stats);
        Assert.Equal(0m, stats.Average);
    }

    [Fact]
    public void SummarizeCost_SampleData_ComputesTotalsInOnePass()
    {
        // Act
        CostStatistics stats = _org.Employees.SummarizeCost();

        // Assert
        Assert.Equal(10, stats.Count);
        Assert.Equal(1_204_800m, stats.Total);
        Assert.Equal(48_000m, stats.Min);
        Assert.Equal(222_000m, stats.Max);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(0.01)]
    public void GiveRaise_AtTheLimits_IsAllowed(decimal percent)
    {
        var employee = new FullTimeEmployee("A", "B", null, 1, new(2020, 1, 1), 100_000m);

        employee.GiveRaise(percent);

        Assert.True(employee.AnnualSalary > 100_000m);
    }

    [Theory]
    [InlineData(50.01)]
    [InlineData(-5)]
    public void GiveRaise_OutsideTheLimits_Throws(decimal percent)
    {
        var employee = new FullTimeEmployee("A", "B", null, 1, new(2020, 1, 1), 100_000m);

        Assert.Throws<DomainException>(() => employee.GiveRaise(percent));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("EN-G")]
    [InlineData("ABCDEFGHIJK")] // 11 characters, the limit is 10
    public void Department_InvalidCode_Throws(string code)
    {
        Assert.Throws<ArgumentException>(() => new Department("Engineering", code, 0));
    }

    [Fact]
    public void Department_LowercaseCode_IsNormalisedToUppercase()
    {
        Assert.Equal("ENG", new Department("Engineering", " eng ", 0).Code);
    }

    [Fact]
    public void Contractor_EndDateNotAfterStart_Throws()
    {
        var start = new DateOnly(2025, 1, 1);

        Assert.Throws<ArgumentException>(() => new Contractor("A", "B", null, 1, start, 50m, 1_000, contractEnd: start));
    }

    [Fact]
    public void Contractor_IsActiveOnLastDayOfContract()
    {
        var contractor = new Contractor("A", "B", null, 1, new(2025, 1, 1), 50m, 1_000, new(2025, 12, 31));

        Assert.True(contractor.IsActiveOn(new(2025, 12, 31)));
        Assert.False(contractor.IsActiveOn(new(2026, 1, 1)));
    }
}
