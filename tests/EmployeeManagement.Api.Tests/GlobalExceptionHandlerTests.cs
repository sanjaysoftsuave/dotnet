using EmployeeManagement.Api.ErrorHandling;
using EmployeeManagement.Application.Common;
using EmployeeManagement.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Api.Tests;

public class GlobalExceptionHandlerTests
{
    public static TheoryData<Exception, int> Mappings => new()
    {
        { new RequestValidationException("Email", "Bad email."), 400 },
        { new DomainException("Rule broken."), 400 },
        { new NotFoundException("Employee", 7), 404 },
        { new ConflictException("Already exists."), 409 },
        { new InvalidOperationException("SELECT * FROM secrets"), 500 },
    };

    [Theory]
    [MemberData(nameof(Mappings))]
    public void ToProblemDetails_MapsExceptionToStatusCode(Exception exception, int expectedStatus)
    {
        ProblemDetails problem = GlobalExceptionHandler.ToProblemDetails(exception);

        Assert.Equal(expectedStatus, problem.Status);
    }

    [Fact]
    public void ToProblemDetails_ValidationException_KeepsFieldErrors()
    {
        var problem = Assert.IsType<ValidationProblemDetails>(
            GlobalExceptionHandler.ToProblemDetails(new RequestValidationException("Email", "Bad email.")));

        Assert.Equal(["Bad email."], problem.Errors["Email"]);
    }

    [Fact]
    public void ToProblemDetails_UnexpectedException_DoesNotLeakTheMessage()
    {
        ProblemDetails problem = GlobalExceptionHandler.ToProblemDetails(new InvalidOperationException("SELECT * FROM secrets"));

        Assert.Null(problem.Detail);
    }
}
