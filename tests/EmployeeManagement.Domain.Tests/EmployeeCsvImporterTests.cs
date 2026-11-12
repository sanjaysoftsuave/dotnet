using EmployeeManagement.Domain.Import;
using EmployeeManagement.Domain.Repositories;
using EmployeeManagement.Domain.Services;

namespace EmployeeManagement.Domain.Tests;

public class EmployeeCsvImporterTests
{
    private readonly InMemoryEmployeeRepository _employees = new();
    private readonly InMemoryDepartmentRepository _departments = new();
    private readonly EmployeeCsvImporter _importer;

    public EmployeeCsvImporterTests()
    {
        _importer = new EmployeeCsvImporter(new OrganizationService(_employees, _departments), _departments);
    }

    [Fact]
    public async Task ImportAsync_ValidRows_ImportsEmployeesAndLinksLaterManagers()
    {
        // Arrange: employee 1 is managed by employee 2, who appears later in the file
        var csv = Csv(
            "1,Rahul,Menon,rahul@example.com,Sales,76000,2021-11-01,2",
            "2,Arjun,Pillai,arjun@example.com,Sales,121000,2017-09-25,");

        // Act
        ImportResult result = await _importer.ImportAsync(csv);

        // Assert
        Assert.Equal(2, result.Imported);
        Assert.Empty(result.Errors);
        Assert.Equal(2, _employees.GetById(1)!.ManagerId);
        Assert.NotNull(_departments.GetByName("Sales"));
    }

    [Fact]
    public async Task ImportAsync_InvalidRow_SkipsItAndReportsLineNumber()
    {
        // Arrange
        var csv = Csv(
            "1,Rahul,Menon,,Sales,76000,2021-11-01,",
            "2,Bad,Row,,Sales,not-a-number,2021-11-01,");

        // Act
        ImportResult result = await _importer.ImportAsync(csv);

        // Assert
        Assert.Equal(1, result.Imported);
        string error = Assert.Single(result.Errors);
        Assert.StartsWith("Line 3:", error);
    }

    [Fact]
    public async Task ImportAsync_CancelledToken_ThrowsAndImportsNothing()
    {
        // Arrange
        var csv = Csv("1,Rahul,Menon,,Sales,76000,2021-11-01,");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _importer.ImportAsync(csv, cancellation.Token));
        Assert.Empty(_employees.GetAll());
    }

    private static StringReader Csv(params string[] rows) =>
        new(string.Join(Environment.NewLine, [EmployeeCsvRow.Header, .. rows]));
}
