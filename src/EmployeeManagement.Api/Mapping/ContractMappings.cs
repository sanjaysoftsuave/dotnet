using EmployeeManagement.Api.Contracts;
using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Employees;

namespace EmployeeManagement.Api.Mapping;

// Manual mapping: explicit, easy to debug, and the compiler catches renamed properties.
public static class ContractMappings
{
    public static EmployeeResponse ToResponse(this Employee employee)
    {
        var response = new EmployeeResponse(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.DepartmentId,
            employee.ManagerId,
            employee.HireDate,
            employee.EmploymentType,
            employee.CalculateAnnualCost());

        return employee switch
        {
            FullTimeEmployee f => response with { AnnualSalary = f.AnnualSalary },
            Contractor c => response with { HourlyRate = c.HourlyRate, HoursPerYear = c.HoursPerYear, ContractEnd = c.ContractEnd },
            _ => response,
        };
    }

    // Assumes the request has passed validation, so the required values are present.
    public static Employee ToEmployee(this CreateEmployeeRequest request)
    {
        Employee employee = request.EmploymentType switch
        {
            EmploymentType.FullTime => new FullTimeEmployee(
                request.FirstName, request.LastName, request.Email, request.DepartmentId, request.HireDate!.Value,
                request.AnnualSalary!.Value),
            EmploymentType.Contractor => new Contractor(
                request.FirstName, request.LastName, request.Email, request.DepartmentId, request.HireDate!.Value,
                request.HourlyRate!.Value, request.HoursPerYear!.Value, request.ContractEnd!.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request.EmploymentType, "Unknown employment type."),
        };

        employee.AssignManager(request.ManagerId);
        return employee;
    }

    public static DepartmentResponse ToResponse(this Department department) =>
        new(department.Id, department.Name, department.Code, department.AnnualBudget, department.HeadEmployeeId);

    public static Department ToDepartment(this CreateDepartmentRequest request) =>
        new(request.Name, request.Code, request.AnnualBudget);
}
