using EmployeeManagement.Domain.Common;

namespace EmployeeManagement.Domain.Repositories;

// Generic base: the storage logic is written once for any Entity type.
public abstract class InMemoryRepository<T> where T : Entity
{
    private readonly Dictionary<int, T> _items = new();
    private int _nextId = 1;

    protected IEnumerable<T> Items => _items.Values;

    public virtual T Add(T item)
    {
        if (item.Id == 0)
        {
            item.AssignId(_nextId);
        }

        if (!_items.TryAdd(item.Id, item))
        {
            throw new InvalidOperationException($"{typeof(T).Name} {item.Id} already exists.");
        }

        _nextId = Math.Max(_nextId, item.Id + 1);
        return item;
    }

    public T? GetById(int id) => _items.GetValueOrDefault(id);

    public IReadOnlyList<T> GetAll() => _items.Values.OrderBy(item => item.Id).ToList();

    public bool Remove(int id) => _items.Remove(id);
}
