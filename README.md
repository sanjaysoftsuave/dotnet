# Employee & Department Management (.NET training capstone)

Capstone project for the 3-week .NET Engineering Bridge. The domain is **Employee & Department Management**.

## Requirements

- .NET SDK 10.0 (pinned in [global.json](global.json), any 10.0.x feature band works)

## Project structure

| Path | Purpose |
|---|---|
| `src/EmployeeManagement.Domain` | Domain model (employees, departments), repository interfaces with in-memory implementations, organisation rules, LINQ queries, async CSV importer |
| `src/EmployeeManagement.ConsoleApp` | Thin console shell: argument parsing, cancellation, report formatting |
| `src/EmployeeManagement.Api` | ASP.NET Core Web API: controllers, middleware, error handling, composition root |
| `src/EmployeeManagement.Application` | Use cases: DTOs, validation, services, repository interfaces |
| `src/EmployeeManagement.Infrastructure` | Repository implementations |
| `tests/EmployeeManagement.Domain.Tests` | xUnit tests (normal, boundary, invalid and regression) |
| `tests/EmployeeManagement.Application.Tests` | Service, validation and mapping tests |
| `tests/EmployeeManagement.Api.Tests` | Middleware, error handling and DI tests |
| `data/employees.csv` | Sample input (the last row is invalid on purpose) |
| `docs/week1` | Daily notes |

## Run

```bash
dotnet build
dotnet run --project src/EmployeeManagement.ConsoleApp -- <csv-file> <command> [arguments]
dotnet run --project src/EmployeeManagement.ConsoleApp -- import <csv-file>
dotnet run --project src/EmployeeManagement.ConsoleApp -- linq
```

| Command | Description |
|---|---|
| `list` | All employees |
| `summary` | Cost statistics and head count per department |
| `search <text>` | Employees whose name contains the text |
| `dept <name>` | Employees in a department |
| `hired <from> <to>` | Employees hired between two dates (`yyyy-MM-dd`, inclusive) |
| `raise <percent>` | Raise for every full-time employee (0 < percent ≤ 50) |
| `import` | Load the file and report imported and skipped rows |
| `linq` | The Day 3 LINQ exercises run against built-in sample data |

Invalid rows are skipped and reported on stderr. Ctrl+C cancels loading.
Exit codes: `0` ok, `1` bad input, `2` file not found, `3` cancelled.

## Run the Web API

```bash
dotnet run --project src/EmployeeManagement.Api --launch-profile http
```

- Swagger UI: http://localhost:5006/swagger
- OpenAPI document: http://localhost:5006/openapi/v1.json
- Example requests: [src/EmployeeManagement.Api/EmployeeManagement.Api.http](src/EmployeeManagement.Api/EmployeeManagement.Api.http)

## Test

```bash
dotnet test
```

## Sample inputs and outputs

**1. Summary**

```text
> dotnet run --project src/EmployeeManagement.ConsoleApp -- data/employees.csv summary
Skipped Line 10: 'not-a-number' is not a valid salary.
Employees  : 8
Total cost : 1,036,800.00
Average    : 129,600.00
Min / Max  : 76,800.00 / 222,000.00
By department:
  Engineering  3 staff     510,000.00
  Sales        2 staff     236,400.00
  Support      2 staff     184,800.00
  HR           1 staff     105,600.00
```

**2. Hired between two dates (both ends inclusive)**

```text
> dotnet run --project src/EmployeeManagement.ConsoleApp -- data/employees.csv hired 2019-03-18 2021-11-01
Skipped Line 10: 'not-a-number' is not a valid salary.
Id   Name                 Department    Annual cost Hired
--------------------------------------------------------------
2    Vikram Iyer          Engineering    170,400.00 2020-07-15
4    Rahul Menon          Sales           91,200.00 2021-11-01
5    Divya Krishnan       HR             105,600.00 2019-03-18
3 employee(s)
```

**3. Invalid input**

```text
> dotnet run --project src/EmployeeManagement.ConsoleApp -- data/employees.csv raise 60
Skipped Line 10: 'not-a-number' is not a valid salary.
A raise must be greater than 0% and at most 50%.
(exit code 1)

> dotnet run --project src/EmployeeManagement.ConsoleApp -- missing.csv list
Input file 'missing.csv' was not found.
(exit code 2)
```

## Daily notes

- Day 1: [Value vs reference types](docs/week1/day1-value-vs-reference-types.md)
- Day 2: [Domain model, class diagram and SOLID](docs/week1/day2-domain-design.md)
- Day 3: [LINQ queries with inputs, outputs and complexity](docs/week1/day3-linq-queries.md)
- Day 4: [Testing, async and tooling](docs/week1/day4-testing-and-async.md) and the [build/test log](docs/week1/day4-build-test-log.txt)
- Day 5: [Gate 1 walkthrough, defect fix and test strategy](docs/week1/day5-gate1-walkthrough.md)

### Week 2

- Day 6: [REST API, status codes, request lifecycle](docs/week2/day6-rest-api.md)
- Day 7: [DI lifetimes, middleware pipeline, configuration and logging](docs/week2/day7-di-and-middleware.md)
- Day 8: [Layered architecture, validation and error handling](docs/week2/day8-layered-architecture.md)
