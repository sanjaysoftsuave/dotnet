using EmployeeManagement.Domain.Import;

namespace EmployeeManagement.Domain.Tests;

public class EmployeeCsvRowTests
{
    [Fact]
    public void Parse_ValidLine_ReturnsAllFields()
    {
        // Act
        EmployeeCsvRow row = EmployeeCsvRow.Parse("2,Vikram,Iyer,vikram@example.com,Engineering,142000.50,2020-07-15,1");

        // Assert
        Assert.Equal(new EmployeeCsvRow(2, "Vikram", "Iyer", "vikram@example.com", "Engineering", 142_000.50m, new(2020, 7, 15), 1), row);
    }

    [Fact]
    public void Parse_OptionalFieldsEmptyAndZeroSalary_AcceptsBoundaryValues()
    {
        // Act
        EmployeeCsvRow row = EmployeeCsvRow.Parse("1,Meera,Nair,,Engineering,0,2023-01-09,");

        // Assert
        Assert.Null(row.Email);
        Assert.Null(row.ManagerId);
        Assert.Equal(0m, row.Salary);
    }

    [Theory]
    [InlineData("1,Meera,Nair,,Engineering,98000,2023-01-09", "columns")]          // too few columns
    [InlineData("0,Meera,Nair,,Engineering,98000,2023-01-09,", "id")]              // id must be positive
    [InlineData("x,Meera,Nair,,Engineering,98000,2023-01-09,", "id")]
    [InlineData("1,Meera,Nair,,Engineering,-1,2023-01-09,", "salary")]             // negative salary
    [InlineData("1,Meera,Nair,,Engineering,abc,2023-01-09,", "salary")]
    [InlineData("1,Meera,Nair,,Engineering,98000,09/01/2023,", "hire date")]       // wrong date format
    [InlineData("1,Meera,Nair,,Engineering,98000,2023-02-30,", "hire date")]       // impossible date
    [InlineData("1,Meera,Nair,,Engineering,98000,2023-01-09,boss", "manager id")]
    public void Parse_InvalidLine_ThrowsFormatExceptionNamingTheField(string line, string field)
    {
        // Act
        var ex = Assert.Throws<FormatException>(() => EmployeeCsvRow.Parse(line));

        // Assert
        Assert.Contains(field, ex.Message);
    }
}
