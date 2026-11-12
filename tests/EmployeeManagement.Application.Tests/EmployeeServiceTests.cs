using EmployeeManagement.Application.Common;
using EmployeeManagement.Application.Employees;
using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace EmployeeManagement.Application.Tests;

public class EmployeeServiceTests
{
    private static readonly DateOnly Today = new(2026, 11, 12);
    private readonly FakeEmployeeRepository _employees = new();
    private readonly FakeDepartmentRepository _departments = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(Today, TimeOnly.MinValue, TimeSpan.Zero));
        var factory = new EmployeeFactory(clock, Options.Create(new HiringOptions()));
        _service = new EmployeeService(_employees, _departments, _unitOfWork, factory, NullLogger<EmployeeService>.Instance);

        _departments.Add(new Department("Engineering", "ENG", 500_000m));
        _departments.Add(new Department("Sales", "SAL", 200_000m));
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_AddsEmployeeAndSavesOnce()
    {
        // Act
        EmployeeResponse created = await _service.CreateAsync(NewHire(departmentId: 1), CancellationToken.None);

        // Assert
        Assert.Equal(1, created.Id);
        Assert.Single(_employees.Items);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task CreateAsync_UnknownDepartmentAndManager_ReportsBothFieldsAndSavesNothing()
    {
        // Act
        var ex = await Assert.ThrowsAsync<RequestValidationException>(
            () => _service.CreateAsync(NewHire(departmentId: 99, managerId: 42), CancellationToken.None));

        // Assert
        Assert.Equal(["DepartmentId", "ManagerId"], ex.Errors.Keys.Order());
        Assert.Empty(_employees.Items);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task GetAsync_UnknownId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAsync(5, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_SelfAsManager_IsRejectedAndEmployeeUnchanged()
    {
        // Arrange
        EmployeeResponse created = await _service.CreateAsync(NewHire(departmentId: 1), CancellationToken.None);
        var request = new UpdateEmployeeRequest { FirstName = "Changed", LastName = "Name", DepartmentId = 2, ManagerId = created.Id };

        // Act
        await Assert.ThrowsAsync<RequestValidationException>(() => _service.UpdateAsync(created.Id, request, CancellationToken.None));

        // Assert: validation runs before any change, so nothing was half-applied
        Employee employee = _employees.Items.Single();
        Assert.Equal("Priya", employee.FirstName);
        Assert.Equal(1, employee.DepartmentId);
    }

    [Fact]
    public async Task GiveRaiseAsync_Contractor_ThrowsDomainException()
    {
        // Arrange
        var contractor = new Contractor("Omar", "Haddad", null, 1, Today, 45m, 1_000, Today.AddYears(1));
        _employees.Add(contractor);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => _service.GiveRaiseAsync(contractor.Id, new RaiseRequest { Percent = 5 }, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_EmployeeWhoManagesOthers_ThrowsConflict()
    {
        // Arrange
        EmployeeResponse manager = await _service.CreateAsync(NewHire(departmentId: 1), CancellationToken.None);
        await _service.CreateAsync(NewHire(departmentId: 1, managerId: manager.Id), CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(manager.Id, CancellationToken.None));
        Assert.Equal(2, _employees.Items.Count);
    }

    private static CreateEmployeeRequest NewHire(int departmentId, int? managerId = null) => new()
    {
        FirstName = "Priya",
        LastName = "Shah",
        DepartmentId = departmentId,
        ManagerId = managerId,
        HireDate = Today,
        EmploymentType = EmploymentType.FullTime,
        AnnualSalary = 100_000,
    };
}
