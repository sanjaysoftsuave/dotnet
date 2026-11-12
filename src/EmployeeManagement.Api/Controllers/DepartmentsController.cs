using EmployeeManagement.Application.Departments;
using EmployeeManagement.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Produces("application/json")]
[Authorize(Policy = Policies.CanReadData)]
public sealed class DepartmentsController(IDepartmentService departments) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DepartmentResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DepartmentResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await departments.ListAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType<DepartmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentResponse>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await departments.GetAsync(id, cancellationToken));

    [HttpGet("{id:int}/cost")]
    [ProducesResponseType<DepartmentCostResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentCostResponse>> GetCost(int id, CancellationToken cancellationToken) =>
        Ok(await departments.GetCostAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = Policies.CanManageDepartments)]
    [ProducesResponseType<DepartmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DepartmentResponse>> Create(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        DepartmentResponse created = await departments.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.CanManageDepartments)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        await departments.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.CanManageDepartments)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await departments.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
