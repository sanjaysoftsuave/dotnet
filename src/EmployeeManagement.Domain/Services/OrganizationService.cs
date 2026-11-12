using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;
using EmployeeManagement.Domain.Repositories;

namespace EmployeeManagement.Domain.Services;

// Depends on abstractions (DIP) and composes repositories instead of inheriting from them.
public sealed class OrganizationService(IEmployeeRepository employees, IDepartmentRepository departments)
{
    public Employee Hire(Employee employee)
    {
        GetDepartment(employee.DepartmentId);

        if (employee.ManagerId is int managerId)
        {
            GetEmployee(managerId);
        }

        return employees.Add(employee);
    }

    public void Transfer(int employeeId, int departmentId)
    {
        Employee employee = GetEmployee(employeeId);
        GetDepartment(departmentId);
        employee.TransferTo(departmentId);
    }

    public void AppointHead(int departmentId, int employeeId)
    {
        Department department = GetDepartment(departmentId);
        Employee employee = GetEmployee(employeeId);

        if (employee.DepartmentId != departmentId)
        {
            throw new DomainException($"{employee.FullName} does not work in {department.Name}.");
        }

        department.AssignHead(employeeId);
    }

    public decimal GetDepartmentAnnualCost(int departmentId)
    {
        GetDepartment(departmentId);

        decimal total = 0;
        foreach (Employee employee in employees.GetByDepartment(departmentId))
        {
            total += employee.CalculateAnnualCost();
        }

        return total;
    }

    public bool IsOverBudget(int departmentId) =>
        GetDepartmentAnnualCost(departmentId) > GetDepartment(departmentId).AnnualBudget;

    private Employee GetEmployee(int id) =>
        employees.GetById(id) ?? throw new EntityNotFoundException(nameof(Employee), id);

    private Department GetDepartment(int id) =>
        departments.GetById(id) ?? throw new EntityNotFoundException(nameof(Department), id);
}
