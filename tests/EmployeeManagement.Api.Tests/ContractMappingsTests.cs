using EmployeeManagement.Api.Contracts;
using EmployeeManagement.Api.Mapping;
using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Api.Tests;

public class ContractMappingsTests
{
    [Fact]
    public void ToResponse_Contractor_FillsContractFieldsOnly()
    {
        // Arrange
        var contractor = new Contractor("Omar", "Haddad", null, 4, new(2026, 1, 1), 45m, 1_000, new(2026, 12, 31));
        contractor.AssignId(12);

        // Act
        EmployeeResponse response = contractor.ToResponse();

        // Assert
        Assert.Equal(12, response.Id);
        Assert.Equal(EmploymentType.Contractor, response.EmploymentType);
        Assert.Equal(45_000m, response.AnnualCost);
        Assert.Equal(new DateOnly(2026, 12, 31), response.ContractEnd);
        Assert.Null(response.AnnualSalary);
    }

    [Fact]
    public void ToEmployee_FullTimeRequest_CreatesFullTimeEmployeeWithManager()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "Priya", LastName = "Shah", DepartmentId = 1, ManagerId = 1, HireDate = new(2026, 11, 2),
            EmploymentType = EmploymentType.FullTime, AnnualSalary = 110_000,
        };

        // Act
        Employee employee = request.ToEmployee();

        // Assert
        var fullTime = Assert.IsType<FullTimeEmployee>(employee);
        Assert.Equal(110_000m, fullTime.AnnualSalary);
        Assert.Equal(1, fullTime.ManagerId);
    }
}
