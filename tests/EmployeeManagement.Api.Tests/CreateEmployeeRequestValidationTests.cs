using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Api.Contracts;
using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Api.Tests;

public class CreateEmployeeRequestValidationTests
{
    private static readonly DateOnly HireDate = new(2026, 11, 2);

    [Fact]
    public void Validate_ValidFullTimeRequest_HasNoErrors()
    {
        // Arrange
        var request = FullTime();

        // Act
        var errors = Validate(request);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_FullTimeWithoutSalary_ReportsAnnualSalary()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "Priya", LastName = "Shah", DepartmentId = 1, HireDate = HireDate,
            EmploymentType = EmploymentType.FullTime,
        };

        // Act
        var errors = Validate(request);

        // Assert
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateEmployeeRequest.AnnualSalary)));
    }

    [Fact]
    public void Validate_ContractorWithoutTerms_ReportsEveryMissingField()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "Omar", LastName = "Haddad", DepartmentId = 4, HireDate = HireDate,
            EmploymentType = EmploymentType.Contractor,
        };

        // Act
        var fields = Validate(request).SelectMany(e => e.MemberNames).ToArray();

        // Assert
        Assert.Equal(["HourlyRate", "HoursPerYear", "ContractEnd"], fields);
    }

    [Fact]
    public void Validate_ContractEndingOnHireDate_IsRejected()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "Omar", LastName = "Haddad", DepartmentId = 4, HireDate = HireDate,
            EmploymentType = EmploymentType.Contractor, HourlyRate = 45, HoursPerYear = 1_200, ContractEnd = HireDate,
        };

        // Act
        var error = Assert.Single(Validate(request));

        // Assert
        Assert.Equal("Contract end must be after the hire date.", error.ErrorMessage);
    }

    [Theory]
    [InlineData("", "Shah", "a@b.com", 1)]          // missing first name
    [InlineData("Priya", "Shah", "not-an-email", 1)] // bad email
    [InlineData("Priya", "Shah", null, 0)]           // department id must be positive
    public void Validate_InvalidFields_AreRejected(string first, string last, string? email, int departmentId)
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = first, LastName = last, Email = email, DepartmentId = departmentId, HireDate = HireDate,
            EmploymentType = EmploymentType.FullTime, AnnualSalary = 100_000,
        };

        // Act & Assert
        Assert.NotEmpty(Validate(request));
    }

    [Fact]
    public void Validate_MissingHireDateAndType_ReportsRequired()
    {
        // Arrange
        var request = new CreateEmployeeRequest { FirstName = "Priya", LastName = "Shah", DepartmentId = 1 };

        // Act
        var fields = Validate(request).SelectMany(e => e.MemberNames).ToArray();

        // Assert
        Assert.Contains(nameof(CreateEmployeeRequest.HireDate), fields);
        Assert.Contains(nameof(CreateEmployeeRequest.EmploymentType), fields);
    }

    private static CreateEmployeeRequest FullTime() => new()
    {
        FirstName = "Priya",
        LastName = "Shah",
        Email = "priya@example.com",
        DepartmentId = 1,
        HireDate = HireDate,
        EmploymentType = EmploymentType.FullTime,
        AnnualSalary = 110_000,
    };

    // Same rules MVC applies to [ApiController] request bodies.
    private static List<ValidationResult> Validate(object request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results;
    }
}
