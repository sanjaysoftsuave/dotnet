using System.Text.Json.Serialization;
using EmployeeManagement.Api;
using EmployeeManagement.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddEmployeeManagement(builder.Configuration);

var app = builder.Build();

// Outermost first: request logging wraps everything after it, so it sees the final status and timing.
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Employee Management API v1"));
}

app.MapControllers();

app.Run();

// Lets the test project reference the entry point (WebApplicationFactory<Program>).
public partial class Program;
