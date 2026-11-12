using EmployeeManagement.Application.Employees;
using EmployeeManagement.Domain.Employees;
using EmployeeManagement.Infrastructure.Persistence;
using EmployeeManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Tests;

public sealed class EfEmployeeRepositoryTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();

    [Fact]
    public async Task ListAsync_FilterSortAndPage_RunsInTheDatabase()
    {
        // Arrange
        await using EmployeeManagementDbContext db = _database.CreateContext();
        var repository = new EfEmployeeRepository(db);
        var query = new EmployeeListQuery { DepartmentId = 1, SortBy = EmployeeSortBy.HireDate, Page = 2, PageSize = 2 };

        // Act
        PagedResult<Employee> result = await repository.ListAsync(query, CancellationToken.None);

        // Assert: Engineering by hire date is Asha, Vikram, Meera, Farhan, so page 2 is Meera, Farhan
        Assert.Equal(4, result.TotalCount);
        Assert.Equal([3, 9], result.Items.Select(e => e.Id));
        Assert.IsType<Contractor>(result.Items[1]); // TPH discriminator materialises the right subclass
    }

    [Fact]
    public async Task ListAsync_CaseInsensitiveSearch_IsTranslatedToSql()
    {
        await using EmployeeManagementDbContext db = _database.CreateContext();

        PagedResult<Employee> result = await new EfEmployeeRepository(db)
            .ListAsync(new EmployeeListQuery { Search = "KRISHNAN" }, CancellationToken.None);

        Assert.Equal(5, Assert.Single(result.Items).Id);
    }

    [Fact]
    public void ListQuery_GeneratedSql_FiltersAndPagesInTheDatabase()
    {
        // Arrange
        using EmployeeManagementDbContext db = _database.CreateContext();
        var query = new EmployeeListQuery { DepartmentId = 1, Search = "an", Page = 2, PageSize = 5 };

        // Act
        string sql = db.Employees.AsNoTracking().ApplyFilters(query).ApplySorting(query).ApplyPaging(query).ToQueryString();

        // Assert: WHERE, ORDER BY and LIMIT/OFFSET are all in SQL, not done in memory afterwards
        Assert.Contains("WHERE", sql);
        Assert.Contains("lower(", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("LIMIT", sql);
        Assert.Contains("OFFSET", sql);
    }

    [Fact]
    public async Task TrackedEntity_ChangeIsSaved_NoTrackingEntity_ChangeIsIgnored()
    {
        // Arrange
        await using (EmployeeManagementDbContext db = _database.CreateContext())
        {
            var tracked = (FullTimeEmployee)(await db.Employees.SingleAsync(e => e.Id == 2));
            var untracked = (FullTimeEmployee)(await db.Employees.AsNoTracking().SingleAsync(e => e.Id == 6));

            // Act
            tracked.GiveRaise(10);
            untracked.GiveRaise(10);
            await db.SaveChangesAsync();
        }

        // Assert (fresh context, so values come from the database)
        await using EmployeeManagementDbContext verify = _database.CreateContext();
        Assert.Equal(156_200m, ((FullTimeEmployee)await verify.Employees.SingleAsync(e => e.Id == 2)).AnnualSalary);
        Assert.Equal(121_000m, ((FullTimeEmployee)await verify.Employees.SingleAsync(e => e.Id == 6)).AnnualSalary);
    }

    [Fact]
    public async Task DeleteDepartmentWithStaff_IsBlockedByForeignKey()
    {
        // Arrange
        await using EmployeeManagementDbContext db = _database.CreateContext();
        var engineering = await db.Departments.SingleAsync(d => d.Id == 1);

        // Act
        db.Departments.Remove(engineering);

        // Assert: DeleteBehavior.Restrict keeps referential integrity even if the service check were skipped
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    public void Dispose() => _database.Dispose();
}
