using EmployeeManagement.Application.Employees;
using EmployeeManagement.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

// HTTP only: routing, binding and choosing the success status code. Rules live in IEmployeeService,
// and failures are thrown as exceptions that GlobalExceptionHandler maps to ProblemDetails.
[ApiController]
[Route("api/employees")]
[Produces("application/json")]
[Authorize(Policy = Policies.CanReadData)]
public sealed class EmployeesController(IEmployeeService employees) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<EmployeeResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<EmployeeResponse>>> GetAll(
        [FromQuery] EmployeeListQuery query, CancellationToken cancellationToken) =>
        Ok(await employees.ListAsync(query, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await employees.GetAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = Policies.CanManageEmployees)]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeResponse>> Create(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        EmployeeResponse created = await employees.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.CanManageEmployees)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        await employees.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/raise")]
    [Authorize(Policy = Policies.CanManageEmployees)]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GiveRaise(int id, RaiseRequest request, CancellationToken cancellationToken) =>
        Ok(await employees.GiveRaiseAsync(id, request, cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.CanManageEmployees)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await employees.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
