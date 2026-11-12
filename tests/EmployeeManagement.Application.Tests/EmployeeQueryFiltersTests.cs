using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Application.Employees;
using EmployeeManagement.Domain.Employees;
using EmployeeManagement.Domain.Samples;

namespace EmployeeManagement.Application.Tests;

// The same filter/sort rules EF Core turns into SQL, run here in memory over the sample data.
public class EmployeeQueryFiltersTests
{
    private readonly IQueryable<Employee> _staff = SampleData.Create().Employees.AsQueryable();

    [Fact]
    public void ApplyFilters_NoCriteria_ReturnsEveryone()
    {
        Assert.Equal(10, _staff.ApplyFilters(new EmployeeListQuery()).Count());
    }

    [Fact]
    public void ApplyFilters_DepartmentAndType_Combine()
    {
        var query = new EmployeeListQuery { DepartmentId = 1, EmploymentType = EmploymentType.Contractor };

        Assert.Equal([9], Ids(_staff.ApplyFilters(query)));
    }

    [Theory]
    [InlineData("NAIR", new[] { 3 })]          // case-insensitive surname
    [InlineData("asha raman", new[] { 1 })]    // full name across both columns
    [InlineData("divya.k@", new[] { 5 })]      // email
    [InlineData("zzz", new int[0])]
    public void ApplyFilters_Search_MatchesNameOrEmail(string search, int[] expected)
    {
        Assert.Equal(expected, Ids(_staff.ApplyFilters(new EmployeeListQuery { Search = search })));
    }

    [Fact]
    public void ApplyFilters_HireDateRange_IsInclusiveAtBothEnds()
    {
        var query = new EmployeeListQuery { HiredFrom = new(2019, 3, 18), HiredTo = new(2021, 11, 1) };

        Assert.Equal([2, 4, 5], Ids(_staff.ApplyFilters(query)));
    }

    [Fact]
    public void ApplySorting_HireDateDescending_NewestFirst()
    {
        var query = new EmployeeListQuery { SortBy = EmployeeSortBy.HireDate, SortDirection = SortDirection.Desc };

        Assert.Equal([9, 10, 7], Ids(_staff.ApplySorting(query).Take(3)));
    }

    [Fact]
    public void ApplyPaging_SecondPageOfThree_SkipsFirstThree()
    {
        var query = new EmployeeListQuery { Page = 2, PageSize = 3 };

        Assert.Equal([4, 5, 6], Ids(_staff.ApplySorting(query).ApplyPaging(query)));
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, EmployeeListQuery.MaxPageSize + 1)]
    public void Validate_PageOrPageSizeOutOfRange_IsRejected(int page, int pageSize)
    {
        Assert.NotEmpty(Validate(new EmployeeListQuery { Page = page, PageSize = pageSize }));
    }

    [Fact]
    public void Validate_HiredFromAfterHiredTo_IsRejected()
    {
        var errors = Validate(new EmployeeListQuery { HiredFrom = new(2025, 1, 2), HiredTo = new(2025, 1, 1) });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(EmployeeListQuery.HiredFrom)));
    }

    [Theory]
    [InlineData(0, 20, 0, false)]
    [InlineData(20, 20, 1, false)]
    [InlineData(21, 20, 2, true)]
    public void PagedResult_ComputesTotalPagesAndNextPage(int total, int pageSize, int expectedPages, bool hasNext)
    {
        var result = new PagedResult<int>([], Page: 1, pageSize, total);

        Assert.Equal(expectedPages, result.TotalPages);
        Assert.Equal(hasNext, result.HasNextPage);
    }

    private static int[] Ids(IEnumerable<Employee> employees) => employees.Select(e => e.Id).ToArray();

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }
}
