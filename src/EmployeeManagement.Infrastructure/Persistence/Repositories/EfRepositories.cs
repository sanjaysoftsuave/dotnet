using EmployeeManagement.Application.Abstractions;
using EmployeeManagement.Application.Employees;
using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Persistence.Repositories;

// Reads that only return data use AsNoTracking (no change-tracking snapshot, less memory).
// GetByIdAsync stays tracked because callers modify the entity and then call SaveChangesAsync.

internal sealed class EfEmployeeRepository(EmployeeManagementDbContext db) : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        db.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<PagedResult<Employee>> ListAsync(EmployeeListQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Employee> filtered = db.Employees.AsNoTracking().ApplyFilters(query);

        // Two round trips: COUNT for the total, then one page with LIMIT/OFFSET.
        int total = await filtered.CountAsync(cancellationToken);
        List<Employee> page = await filtered.ApplySorting(query).ApplyPaging(query).ToListAsync(cancellationToken);

        return new PagedResult<Employee>(page, query.Page, query.PageSize, total);
    }

    public async Task<IReadOnlyList<Employee>> ListByDepartmentAsync(int departmentId, CancellationToken cancellationToken) =>
        await db.Employees.AsNoTracking()
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) =>
        db.Employees.AnyAsync(e => e.Id == id, cancellationToken);

    public Task<bool> AnyInDepartmentAsync(int departmentId, CancellationToken cancellationToken) =>
        db.Employees.AnyAsync(e => e.DepartmentId == departmentId, cancellationToken);

    public Task<bool> AnyReportingToAsync(int managerId, CancellationToken cancellationToken) =>
        db.Employees.AnyAsync(e => e.ManagerId == managerId, cancellationToken);

    public void Add(Employee employee) => db.Employees.Add(employee);

    public void Remove(Employee employee) => db.Employees.Remove(employee);
}

internal sealed class EfDepartmentRepository(EmployeeManagementDbContext db) : IDepartmentRepository
{
    public Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        db.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken) =>
        await db.Departments.AsNoTracking().OrderBy(d => d.Id).ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) =>
        db.Departments.AnyAsync(d => d.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        string normalized = name.Trim().ToLower();
        return db.Departments.AnyAsync(d => d.Id != excludeId && d.Name.ToLower() == normalized, cancellationToken);
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken)
    {
        string normalized = code.Trim().ToUpperInvariant();
        return db.Departments.AnyAsync(d => d.Code == normalized, cancellationToken);
    }

    public void Add(Department department) => db.Departments.Add(department);

    public void Remove(Department department) => db.Departments.Remove(department);
}
