using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Application.Employees;

// DTOs: the shape clients send and receive. They never expose domain entities directly.
// Attributes check the shape of a request; rules that need data (does the department exist?) live in the services.

public sealed class CreateEmployeeRequest : IValidatableObject
{
    [Required, StringLength(50)]
    public string FirstName { get; init; } = "";

    [Required, StringLength(50)]
    public string LastName { get; init; } = "";

    [EmailAddress, StringLength(254)]
    public string? Email { get; init; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; init; }

    [Range(1, int.MaxValue)]
    public int? ManagerId { get; init; }

    // Nullable so a missing value is reported as "required" instead of defaulting to 0001-01-01.
    [Required]
    public DateOnly? HireDate { get; init; }

    [Required]
    public EmploymentType? EmploymentType { get; init; }

    [Range(typeof(decimal), "0", "100000000")]
    public decimal? AnnualSalary { get; init; }

    [Range(typeof(decimal), "0", "100000")]
    public decimal? HourlyRate { get; init; }

    [Range(1, 4000)]
    public int? HoursPerYear { get; init; }

    public DateOnly? ContractEnd { get; init; }

    // Cross-field rules that attributes cannot express. Runs only after the attribute checks pass.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EmploymentType == Domain.Employees.EmploymentType.FullTime)
        {
            if (AnnualSalary is null)
            {
                yield return new ValidationResult("Annual salary is required for full-time employees.", [nameof(AnnualSalary)]);
            }

            yield break;
        }

        if (HourlyRate is null)
        {
            yield return new ValidationResult("Hourly rate is required for contractors.", [nameof(HourlyRate)]);
        }

        if (HoursPerYear is null)
        {
            yield return new ValidationResult("Hours per year is required for contractors.", [nameof(HoursPerYear)]);
        }

        if (ContractEnd is null)
        {
            yield return new ValidationResult("Contract end is required for contractors.", [nameof(ContractEnd)]);
        }
        else if (ContractEnd <= HireDate)
        {
            yield return new ValidationResult("Contract end must be after the hire date.", [nameof(ContractEnd)]);
        }
    }
}

// PUT replaces every editable field, so all of them must be sent.
public sealed class UpdateEmployeeRequest
{
    [Required, StringLength(50)]
    public string FirstName { get; init; } = "";

    [Required, StringLength(50)]
    public string LastName { get; init; } = "";

    [EmailAddress, StringLength(254)]
    public string? Email { get; init; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; init; }

    [Range(1, int.MaxValue)]
    public int? ManagerId { get; init; }
}

public sealed class RaiseRequest
{
    [Range(typeof(decimal), "0.01", "50")]
    public decimal Percent { get; init; }
}

public sealed record EmployeeResponse(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    int DepartmentId,
    int? ManagerId,
    DateOnly HireDate,
    EmploymentType EmploymentType,
    decimal AnnualCost)
{
    public decimal? AnnualSalary { get; init; }
    public decimal? HourlyRate { get; init; }
    public int? HoursPerYear { get; init; }
    public DateOnly? ContractEnd { get; init; }
}
