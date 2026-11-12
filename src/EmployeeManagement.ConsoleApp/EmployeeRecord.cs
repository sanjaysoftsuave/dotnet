namespace EmployeeManagement.ConsoleApp;

// Reference type: variables hold a reference, so every holder sees changes made through any of them.
public sealed class EmployeeRecord
{
    public int Id { get; init; }                     // value type (System.Int32)
    public string FirstName { get; init; } = "";      // reference type, immutable
    public string LastName { get; init; } = "";
    public string? Email { get; init; }               // nullable reference type: may be missing
    public string Department { get; init; } = "";
    public decimal Salary { get; set; }               // value type (System.Decimal)
    public DateOnly HireDate { get; init; }           // value type (struct)
    public int? ManagerId { get; init; }              // nullable value type: Nullable<int>

    public string FullName => $"{FirstName} {LastName}";
}
