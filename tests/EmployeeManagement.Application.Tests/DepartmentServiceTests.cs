using EmployeeManagement.Application.Common;
using EmployeeManagement.Application.Departments;
using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Application.Tests;

public class DepartmentServiceTests
{
    private readonly FakeEmployeeRepository _employees = new();
    private readonly FakeDepartmentRepository _departments = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _service = new DepartmentService(_departments, _employees, _unitOfWork);
        _departments.Add(new Department("Engineering", "ENG", 200_000m));
    }

    [Theory]
    [InlineData("engineering", "NEW")] // name clash, case-insensitive
    [InlineData("Finance", "eng")]     // code clash
    public async Task CreateAsync_DuplicateNameOrCode_ThrowsConflict(string name, string code)
    {
        var request = new CreateDepartmentRequest { Name = name, Code = code, AnnualBudget = 1 };

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAsync(request, CancellationToken.None));
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task UpdateAsync_KeepingItsOwnName_IsAllowed()
    {
        await _service.UpdateAsync(1, new UpdateDepartmentRequest { Name = "Engineering", AnnualBudget = 250_000m }, CancellationToken.None);

        Assert.Equal(250_000m, _departments.Items[0].AnnualBudget);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task DeleteAsync_DepartmentWithStaff_ThrowsConflict()
    {
        _employees.Add(new FullTimeEmployee("Asha", "Raman", null, 1, new(2020, 1, 1), 100_000m));

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync(1, CancellationToken.None));
        Assert.Single(_departments.Items);
    }

    [Fact]
    public async Task GetCostAsync_StaffCostAboveBudget_FlagsOverBudget()
    {
        // Arrange: 2 x 100,000 x 1.2 = 240,000 against a 200,000 budget
        _employees.Add(new FullTimeEmployee("Asha", "Raman", null, 1, new(2020, 1, 1), 100_000m));
        _employees.Add(new FullTimeEmployee("Vikram", "Iyer", null, 1, new(2021, 1, 1), 100_000m));

        // Act
        DepartmentCostResponse cost = await _service.GetCostAsync(1, CancellationToken.None);

        // Assert
        Assert.Equal(2, cost.HeadCount);
        Assert.Equal(240_000m, cost.TotalCost);
        Assert.True(cost.IsOverBudget);
    }
}
