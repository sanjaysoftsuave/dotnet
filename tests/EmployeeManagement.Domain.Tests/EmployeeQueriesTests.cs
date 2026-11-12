using EmployeeManagement.Domain.Employees;
using EmployeeManagement.Domain.Queries;
using EmployeeManagement.Domain.Samples;

namespace EmployeeManagement.Domain.Tests;

public class EmployeeQueriesTests
{
    private readonly SampleOrganization _org = SampleData.Create();

    [Fact]
    public void TopByCost_ThreeRequested_ReturnsHighestCostsInOrder()
    {
        // Act
        int[] ids = _org.Employees.TopByCost(3).Select(e => e.Id).ToArray();

        // Assert
        Assert.Equal([1, 2, 6], ids);
    }

    [Fact]
    public void CostByDepartment_SampleData_FlagsEngineeringAndSupportOverBudget()
    {
        // Act
        var overBudget = _org.Employees.CostByDepartment(_org.Departments)
            .Where(s => s.IsOverBudget)
            .Select(s => s.Department);

        // Assert
        Assert.Equal(["Engineering", "Support"], overBudget);
    }

    [Fact]
    public void GroupBySalaryBand_SampleData_PutsContractorsInContractBand()
    {
        // Act
        var bands = _org.Employees.GroupBySalaryBand().ToDictionary(g => g.Key, g => g.Count());

        // Assert
        Assert.Equal(3, bands[SalaryBand.Junior]);
        Assert.Equal(4, bands[SalaryBand.Mid]);
        Assert.Equal(1, bands[SalaryBand.Senior]);
        Assert.Equal(2, bands[SalaryBand.Contract]);
    }

    [Fact]
    public void Page_SecondPageOfFour_SkipsFirstFour()
    {
        // Act
        int[] ids = _org.Employees.Page(pageNumber: 2, pageSize: 4).Select(e => e.Id).ToArray();

        // Assert
        Assert.Equal([5, 6, 7, 8], ids);
    }

    [Fact]
    public void SearchByName_IsDeferred_SeesItemsAddedAfterTheQueryIsBuilt()
    {
        // Arrange
        var staff = new List<Employee>(_org.Employees);
        IEnumerable<Employee> query = staff.SearchByName("zara");

        // Act: the query has not run yet, so the new employee is included
        staff.Add(new FullTimeEmployee("Zara", "Khan", null, 1, new DateOnly(2025, 1, 1), 70_000m));

        // Assert
        Assert.Single(query);
    }
}
