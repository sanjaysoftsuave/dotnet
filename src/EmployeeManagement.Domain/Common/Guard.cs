namespace EmployeeManagement.Domain.Common;

internal static class Guard
{
    public static string NotBlank(string? value, string paramName, int maxLength = 100)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", paramName);
        }

        string trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Value must be at most {maxLength} characters.", paramName);
        }

        return trimmed;
    }

    public static decimal NotNegative(decimal value, string paramName) =>
        value >= 0 ? value : throw new ArgumentOutOfRangeException(paramName, value, "Value cannot be negative.");

    public static int Positive(int value, string paramName) =>
        value > 0 ? value : throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
}
