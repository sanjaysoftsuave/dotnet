using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Departments;

namespace EmployeeManagement.Application.Departments;

public sealed class CreateDepartmentRequest
{
    [Required, StringLength(60)]
    public string Name { get; init; } = "";

    [Required, StringLength(10), RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Code may only contain letters and digits.")]
    public string Code { get; init; } = "";

    [Range(typeof(decimal), "0", "1000000000")]
    public decimal AnnualBudget { get; init; }
}

public sealed class UpdateDepartmentRequest
{
    [Required, StringLength(60)]
    public string Name { get; init; } = "";

    [Range(typeof(decimal), "0", "1000000000")]
    public decimal AnnualBudget { get; init; }
}

public sealed record DepartmentResponse(int Id, string Name, string Code, decimal AnnualBudget, int? HeadEmployeeId);

public sealed record DepartmentCostResponse(int DepartmentId, string Department, int HeadCount, decimal TotalCost, decimal Budget, bool IsOverBudget);

public static class DepartmentMappings
{
    public static DepartmentResponse ToResponse(this Department department) =>
        new(department.Id, department.Name, department.Code, department.AnnualBudget, department.HeadEmployeeId);
}
