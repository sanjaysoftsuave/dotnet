using EmployeeManagement.Domain.Common;

namespace EmployeeManagement.Domain.Departments;

public sealed class Department : Entity
{
    public Department(string name, string code, decimal annualBudget)
    {
        Rename(name);
        Code = NormalizeCode(code);
        ChangeBudget(annualBudget);
    }

    public string Name { get; private set; } = "";
    public string Code { get; }
    public decimal AnnualBudget { get; private set; }
    public int? HeadEmployeeId { get; private set; }

    public void Rename(string name) => Name = Guard.NotBlank(name, nameof(name), 60);

    public void ChangeBudget(decimal annualBudget) => AnnualBudget = Guard.NotNegative(annualBudget, nameof(annualBudget));

    public void AssignHead(int employeeId) => HeadEmployeeId = Guard.Positive(employeeId, nameof(employeeId));

    private static string NormalizeCode(string code)
    {
        string trimmed = Guard.NotBlank(code, nameof(code), 10).ToUpperInvariant();
        foreach (char c in trimmed)
        {
            if (!char.IsAsciiLetterOrDigit(c))
            {
                throw new ArgumentException("Code may only contain letters and digits.", nameof(code));
            }
        }

        return trimmed;
    }

    public override string ToString() => $"{Code} - {Name}";
}
