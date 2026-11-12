namespace EmployeeManagement.Domain.Employees;

// Record: immutable, compared by value. Two addresses with the same fields are equal.
public sealed record Address(string Street, string City, string PostalCode, string Country = "India");
