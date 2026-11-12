using EmployeeManagement.Domain.Queries;
using EmployeeManagement.Domain.Samples;

namespace EmployeeManagement.Domain.Tests;

public class HiredBetweenTests
{
    private readonly SampleOrganization _org = SampleData.Create();

    [Fact]
    public void HiredBetween_RangeCoveringSeveralHires_ReturnsThem()
    {
        // Act
        int[] ids = _org.Employees.HiredBetween(new(2019, 1, 1), new(2021, 12, 31)).Select(e => e.Id).ToArray();

        // Assert
        Assert.Equal([2, 4, 5], ids);
    }

    [Fact]
    public void HiredBetween_EmployeeHiredOnStartDate_IsIncluded()
    {
        // Act: Divya Krishnan (id 5) was hired on 2019-03-18
        var result = _org.Employees.HiredBetween(new(2019, 3, 18), new(2019, 6, 30));

        // Assert
        Assert.Equal(5, Assert.Single(result).Id);
    }

    // Regression test for defect DEF-001: HiredBetween compared the end date with '<',
    // so anyone hired exactly on the 'to' date was missing from reports.
    [Fact]
    public void HiredBetween_EmployeeHiredOnEndDate_IsIncluded()
    {
        // Act: Rahul Menon (id 4) was hired on 2021-11-01
        var result = _org.Employees.HiredBetween(new(2021, 6, 1), new(2021, 11, 1));

        // Assert
        Assert.Equal(4, Assert.Single(result).Id);
    }

    [Fact]
    public void HiredBetween_SameStartAndEndDate_ReturnsHiresOnThatDay()
    {
        // Act
        var result = _org.Employees.HiredBetween(new(2016, 6, 30), new(2016, 6, 30));

        // Assert
        Assert.Equal(8, Assert.Single(result).Id);
    }

    [Fact]
    public void HiredBetween_FromAfterTo_ThrowsImmediately()
    {
        // Act & Assert: thrown on call, not deferred until enumeration
        Assert.Throws<ArgumentException>(() => _org.Employees.HiredBetween(new(2022, 1, 1), new(2021, 1, 1)));
    }
}
