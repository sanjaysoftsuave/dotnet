using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Controllers;

public sealed class DemoTokenRequest
{
    [Required, StringLength(50)]
    public string UserName { get; init; } = "";

    [Required]
    public string Role { get; init; } = "";
}

// DEVELOPMENT ONLY: hands out a token for any known role without a password so the 200/401/403 cases
// can be demonstrated. It returns 404 in every other environment. Production would use a real identity provider.
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(ITokenService tokens, IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost("token")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TokenResponse> CreateDemoToken(DemoTokenRequest request)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        if (!Roles.All.Contains(request.Role))
        {
            ModelState.AddModelError(nameof(request.Role), $"Role must be one of: {string.Join(", ", Roles.All)}.");
            return ValidationProblem(ModelState);
        }

        return Ok(tokens.CreateToken(request.UserName, [request.Role]));
    }
}
