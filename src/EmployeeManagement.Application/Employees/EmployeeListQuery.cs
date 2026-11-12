using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Application.Employees;

public enum EmployeeSortBy
{
    Id,
    LastName,
    HireDate,
}

public enum SortDirection
{
    Asc,
    Desc,
}

// Bound from the query string: GET /api/employees?departmentId=1&search=an&sortBy=HireDate&page=2&pageSize=5
public sealed class EmployeeListQuery : IValidatableObject
{
    public const int MaxPageSize = 100;

    [Range(1, int.MaxValue)]
    public int? DepartmentId { get; init; }

    public EmploymentType? EmploymentType { get; init; }

    [StringLength(50)]
    public string? Search { get; init; }

    public DateOnly? HiredFrom { get; init; }

    public DateOnly? HiredTo { get; init; }

    public EmployeeSortBy SortBy { get; init; } = EmployeeSortBy.Id;

    public SortDirection SortDirection { get; init; } = SortDirection.Asc;

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, MaxPageSize)]
    public int PageSize { get; init; } = 20;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HiredFrom > HiredTo)
        {
            yield return new ValidationResult("hiredFrom must be on or before hiredTo.", [nameof(HiredFrom)]);
        }
    }
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;

    public PagedResult<TOut> Map<TOut>(Func<T, TOut> map) => new(Items.Select(map).ToList(), Page, PageSize, TotalCount);
}
