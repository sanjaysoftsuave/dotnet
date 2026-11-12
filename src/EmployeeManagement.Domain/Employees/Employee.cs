using EmployeeManagement.Domain.Common;

namespace EmployeeManagement.Domain.Employees;

// Abstract class: shares state and behaviour between employee kinds, but cannot be created on its own.
public abstract class Employee : Entity
{
    protected Employee(string firstName, string lastName, string? email, int departmentId, DateOnly hireDate)
    {
        Rename(firstName, lastName);
        ChangeEmail(email);
        DepartmentId = Guard.Positive(departmentId, nameof(departmentId));
        HireDate = hireDate;
    }

    public string FirstName { get; private set; } = "";
    public string LastName { get; private set; } = "";
    public string? Email { get; private set; }
    public int DepartmentId { get; private set; }
    public int? ManagerId { get; private set; }
    public DateOnly HireDate { get; }
    public Address? Address { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    public abstract EmploymentType EmploymentType { get; }

    // Polymorphism: each subclass prices itself differently.
    public abstract decimal CalculateAnnualCost();

    public void Rename(string firstName, string lastName)
    {
        FirstName = Guard.NotBlank(firstName, nameof(firstName), 50);
        LastName = Guard.NotBlank(lastName, nameof(lastName), 50);
    }

    public void ChangeEmail(string? email)
    {
        if (email is not null && !email.Contains('@'))
        {
            throw new ArgumentException($"'{email}' is not a valid email address.", nameof(email));
        }

        Email = email?.Trim();
    }

    public void MoveTo(Address address) => Address = address;

    public void TransferTo(int departmentId) => DepartmentId = Guard.Positive(departmentId, nameof(departmentId));

    public void AssignManager(int? managerId)
    {
        if (managerId is not null && managerId == Id)
        {
            throw new DomainException("An employee cannot manage themselves.");
        }

        ManagerId = managerId;
    }

    public int YearsOfService(DateOnly asOf)
    {
        int years = asOf.Year - HireDate.Year;
        if (asOf < HireDate.AddYears(years))
        {
            years--;
        }

        return Math.Max(years, 0);
    }

    public override string ToString() => $"#{Id} {FullName} ({EmploymentType})";
}
