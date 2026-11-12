using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;
using EmployeeManagement.Domain.Repositories;
using EmployeeManagement.Domain.Services;

namespace EmployeeManagement.Domain.Tests;

public class OrganizationServiceTests
{
    private readonly InMemoryEmployeeRepository _employees = new();
    private readonly InMemoryDepartmentRepository _departments = new();
    private readonly OrganizationService _service;
    private readonly Department _engineering;
    private readonly Department _sales;

    public OrganizationServiceTests()
    {
        _service = new OrganizationService(_employees, _departments);
        _engineering = _departments.Add(new Department("Engineering", "ENG", 200_000m));
        _sales = _departments.Add(new Department("Sales", "SAL", 100_000m));
    }

    [Fact]
    public void Hire_UnknownDepartment_ThrowsEntityNotFound()
    {
        // Arrange
        var employee = NewEmployee(departmentId: 99);

        // Act
        var ex = Assert.Throws<EntityNotFoundException>(() => _service.Hire(employee));

        // Assert
        Assert.Equal(nameof(Department), ex.EntityName);
        Assert.Empty(_employees.GetAll());
    }

    [Fact]
    public void Transfer_ToOtherDepartment_MovesEmployeeAndRaisesEvent()
    {
        // Arrange
        Employee employee = _service.Hire(NewEmployee(_engineering.Id));
        EmployeeTransferredEventArgs? raised = null;
        _service.EmployeeTransferred += (_, e) => raised = e;

        // Act
        _service.Transfer(employee.Id, _sales.Id);

        // Assert
        Assert.Equal(_sales.Id, employee.DepartmentId);
        Assert.NotNull(raised);
        Assert.Equal(_engineering.Id, raised.FromDepartmentId);
        Assert.Equal(_sales.Id, raised.ToDepartmentId);
    }

    [Fact]
    public void Transfer_ToSameDepartment_DoesNotRaiseEvent()
    {
        // Arrange
        Employee employee = _service.Hire(NewEmployee(_engineering.Id));
        int eventCount = 0;
        _service.EmployeeTransferred += (_, _) => eventCount++;

        // Act
        _service.Transfer(employee.Id, _engineering.Id);

        // Assert
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void AppointHead_EmployeeFromAnotherDepartment_ThrowsDomainException()
    {
        // Arrange
        Employee salesPerson = _service.Hire(NewEmployee(_sales.Id));

        // Act & Assert
        Assert.Throws<DomainException>(() => _service.AppointHead(_engineering.Id, salesPerson.Id));
        Assert.Null(_engineering.HeadEmployeeId);
    }

    [Fact]
    public void IsOverBudget_CostAboveBudget_ReturnsTrue()
    {
        // Arrange: 2 x (90,000 * 1.2) = 216,000 against a 200,000 budget
        _service.Hire(NewEmployee(_engineering.Id, salary: 90_000m));
        _service.Hire(NewEmployee(_engineering.Id, salary: 90_000m));

        // Act
        bool overBudget = _service.IsOverBudget(_engineering.Id);

        // Assert
        Assert.True(overBudget);
        Assert.Equal(216_000m, _service.GetDepartmentAnnualCost(_engineering.Id));
    }

    private static FullTimeEmployee NewEmployee(int departmentId, decimal salary = 50_000m) =>
        new("Test", "Person", "test@example.com", departmentId, new DateOnly(2022, 5, 1), salary);
}
