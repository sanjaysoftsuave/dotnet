using EmployeeManagement.Domain.Samples;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Tests;

// A real SQLite database held in memory. It lives as long as the open connection,
// so every DbContext created from this class sees the same data.
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SqliteTestDatabase()
    {
        _connection.Open();
        using EmployeeManagementDbContext db = CreateContext();
        db.Database.Migrate(); // runs the real migrations, so they are tested too

        SampleOrganization sample = SampleData.Create();
        db.Departments.AddRange(sample.Departments);
        db.Employees.AddRange(sample.Employees);
        db.SaveChanges();
    }

    public EmployeeManagementDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EmployeeManagementDbContext>().UseSqlite(_connection).Options);

    public void Dispose() => _connection.Dispose();
}
