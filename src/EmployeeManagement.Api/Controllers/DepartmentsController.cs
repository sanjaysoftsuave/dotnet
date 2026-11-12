using EmployeeManagement.Api.Contracts;
using EmployeeManagement.Api.Mapping;
using EmployeeManagement.Domain.Departments;
using EmployeeManagement.Domain.Queries;
using EmployeeManagement.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Produces("application/json")]
public sealed class DepartmentsController(IDepartmentRepository departments, IEmployeeRepository employees) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DepartmentResponse>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<DepartmentResponse>> GetAll() =>
        Ok(departments.GetAll().Select(d => d.ToResponse()).ToList());

    [HttpGet("{id:int}")]
    [ProducesResponseType<DepartmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<DepartmentResponse> GetById(int id) =>
        departments.GetById(id) is { } department ? Ok(department.ToResponse()) : NotFound();

    [HttpGet("{id:int}/cost")]
    [ProducesResponseType<DepartmentCostResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<DepartmentCostResponse> GetCost(int id)
    {
        if (departments.GetById(id) is not { } department)
        {
            return NotFound();
        }

        DepartmentCostSummary summary = employees.GetByDepartment(id).CostByDepartment([department]).Single();
        return Ok(new DepartmentCostResponse(id, summary.Department, summary.HeadCount, summary.TotalCost, summary.Budget, summary.IsOverBudget));
    }

    [HttpPost]
    [ProducesResponseType<DepartmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public ActionResult<DepartmentResponse> Create(CreateDepartmentRequest request)
    {
        if (departments.GetByName(request.Name) is not null)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Department already exists.",
                detail: $"A department named '{request.Name}' already exists.");
        }

        Department department = departments.Add(request.ToDepartment());
        return CreatedAtAction(nameof(GetById), new { id = department.Id }, department.ToResponse());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, UpdateDepartmentRequest request)
    {
        if (departments.GetById(id) is not { } department)
        {
            return NotFound();
        }

        department.Rename(request.Name);
        department.ChangeBudget(request.AnnualBudget);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public IActionResult Delete(int id)
    {
        if (departments.GetById(id) is null)
        {
            return NotFound();
        }

        if (employees.GetByDepartment(id).Count > 0)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Department is not empty.",
                detail: "Move or remove its employees before deleting the department.");
        }

        departments.Remove(id);
        return NoContent();
    }
}
