using EmployeeManagement.Api.Contracts;
using EmployeeManagement.Api.Mapping;
using EmployeeManagement.Api.Services;
using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Employees;
using EmployeeManagement.Domain.Repositories;
using EmployeeManagement.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/employees")]
[Produces("application/json")]
public sealed class EmployeesController(
    IEmployeeRepository employees,
    OrganizationService organization,
    IEmployeeFactory factory,
    ILogger<EmployeesController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EmployeeResponse>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<EmployeeResponse>> GetAll() =>
        Ok(employees.GetAll().Select(e => e.ToResponse()).ToList());

    [HttpGet("{id:int}")]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<EmployeeResponse> GetById(int id) =>
        employees.GetById(id) is { } employee ? Ok(employee.ToResponse()) : NotFound();

    [HttpPost]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<EmployeeResponse> Create(CreateEmployeeRequest request)
    {
        try
        {
            Employee employee = organization.Hire(factory.Create(request));
            logger.LogInformation("Hired employee {EmployeeId} into department {DepartmentId}", employee.Id, employee.DepartmentId);
            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee.ToResponse());
        }
        catch (Exception ex) when (ex is DomainException or ArgumentException)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "The request breaks a business rule.", detail: ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, UpdateEmployeeRequest request)
    {
        if (employees.GetById(id) is not { } employee)
        {
            return NotFound();
        }

        try
        {
            organization.Transfer(id, request.DepartmentId);
            organization.AssignManager(id, request.ManagerId);
            employee.Rename(request.FirstName, request.LastName);
            employee.ChangeEmail(request.Email);
            return NoContent();
        }
        catch (Exception ex) when (ex is DomainException or ArgumentException)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "The request breaks a business rule.", detail: ex.Message);
        }
    }

    [HttpPost("{id:int}/raise")]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<EmployeeResponse> GiveRaise(int id, RaiseRequest request)
    {
        switch (employees.GetById(id))
        {
            case null:
                return NotFound();
            case FullTimeEmployee employee:
                employee.GiveRaise(request.Percent);
                logger.LogInformation("Gave employee {EmployeeId} a raise of {Percent}%", id, request.Percent);
                return Ok(employee.ToResponse());
            default:
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "The request breaks a business rule.",
                    detail: "Only full-time employees can receive a raise.");
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        if (!employees.Remove(id))
        {
            return NotFound();
        }

        logger.LogInformation("Removed employee {EmployeeId}", id);
        return NoContent();
    }
}
