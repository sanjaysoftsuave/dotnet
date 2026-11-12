using System.Text.Json.Serialization;
using EmployeeManagement.Api;
using EmployeeManagement.Api.Middleware;
using EmployeeManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddEmployeeManagement(builder.Configuration);

var app = builder.Build();

// Applies migrations and seeds sample data when enabled in configuration (Development).
await app.Services.InitializeDatabaseAsync();

// Order matters: each middleware wraps everything registered after it.
app.UseMiddleware<RequestLoggingMiddleware>(); // 1. outermost, so it logs the final status code
app.UseExceptionHandler();                      // 2. exceptions → ProblemDetails (GlobalExceptionHandler)
app.UseStatusCodePages();                       // 3. empty 4xx responses (e.g. unknown routes) → ProblemDetails

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Employee Management API v1"));
}

app.MapControllers();

await app.RunAsync();

// Lets the test project reference the entry point (WebApplicationFactory<Program>).
public partial class Program;
