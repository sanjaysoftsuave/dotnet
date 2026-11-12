namespace EmployeeManagement.Domain.Common;

public abstract class Entity
{
    public int Id { get; private set; }

    // Ids are issued by whoever stores the entity (a repository today, the database later).
    public void AssignId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), id, "Id must be positive.");
        }

        if (Id != 0 && Id != id)
        {
            throw new DomainException($"{GetType().Name} already has id {Id}.");
        }

        Id = id;
    }
}
