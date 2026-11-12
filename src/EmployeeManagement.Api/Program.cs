using System.Text.Json.Serialization;
using EmployeeManagement.Domain.Repositories;
using EmployeeManagement.Domain.Samples;
using EmployeeManagement.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// In-memory storage seeded with the sample organisation (replaced by EF Core on Day 9).
SampleOrganization sample = SampleData.Create();
var departments = new InMemoryDepartmentRepository();
var employees = new InMemoryEmployeeRepository();
foreach (var department in sample.Departments) departments.Add(department);
foreach (var employee in sample.Employees) employees.Add(employee);

builder.Services.AddSingleton<IDepartmentRepository>(departments);
builder.Services.AddSingleton<IEmployeeRepository>(employees);
builder.Services.AddSingleton<OrganizationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Employee Management API v1"));
}

app.MapControllers();

app.Run();

// Lets the test project reference the entry point (WebApplicationFactory<Program>).
public partial class Program;
