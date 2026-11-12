using EmployeeManagement.Application.Employees;
using EmployeeManagement.Application.Common;
using EmployeeManagement.Domain.Employees;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace EmployeeManagement.Application.Tests;

public class EmployeeFactoryTests
{
    // "Today" is fixed through the injected clock, so these tests never depend on the real date.
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 11, 12, 9, 0, 0, TimeSpan.Zero));
    private readonly EmployeeFactory _factory;

    public EmployeeFactoryTests()
    {
        _factory = new EmployeeFactory(_time, Options.Create(new HiringOptions { MaxDaysInFuture = 30 }));
    }

    [Fact]
    public void Create_HireDateOnTheLimit_IsAllowed()
    {
        // Act
        Employee employee = _factory.Create(Request(new DateOnly(2026, 12, 12)));

        // Assert
        Assert.Equal(new DateOnly(2026, 12, 12), employee.HireDate);
    }

    [Fact]
    public void Create_HireDateBeyondTheLimit_ReportsHireDateError()
    {
        // Act
        var ex = Assert.Throws<RequestValidationException>(() => _factory.Create(Request(new DateOnly(2026, 12, 13))));

        // Assert
        Assert.Contains("2026-12-12", Assert.Single(ex.Errors["HireDate"]));
    }

    [Fact]
    public void Create_AfterClockMovesForward_AcceptsTheLaterDate()
    {
        // Arrange
        _time.Advance(TimeSpan.FromDays(1));

        // Act & Assert: no exception
        _factory.Create(Request(new DateOnly(2026, 12, 13)));
    }

    private static CreateEmployeeRequest Request(DateOnly hireDate) => new()
    {
        FirstName = "Priya",
        LastName = "Shah",
        DepartmentId = 1,
        HireDate = hireDate,
        EmploymentType = EmploymentType.FullTime,
        AnnualSalary = 100_000,
    };
}
