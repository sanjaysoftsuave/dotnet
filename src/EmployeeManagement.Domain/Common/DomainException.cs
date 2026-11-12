namespace EmployeeManagement.Domain.Common;

// Thrown when an operation would break a business rule.
public class DomainException(string message) : Exception(message);

public sealed class EntityNotFoundException(string entityName, int id)
    : DomainException($"{entityName} with id {id} was not found.")
{
    public string EntityName { get; } = entityName;
    public int EntityId { get; } = id;
}
