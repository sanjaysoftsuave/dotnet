# Employee & Department Management (.NET training capstone)

Capstone project for the 3-week .NET Engineering Bridge. The domain is **Employee & Department Management**.

## Requirements

- .NET SDK 10.0 (pinned in [global.json](global.json), any 10.0.x feature band works)

## Project structure

| Path | Purpose |
|---|---|
| `src/EmployeeManagement.ConsoleApp` | Console app that loads employee records from CSV and prints reports |
| `src/EmployeeManagement.Domain` | Domain model (employees, departments), repository interfaces and in-memory implementations, organisation rules |
| `tests/EmployeeManagement.Domain.Tests` | xUnit tests for the domain, queries and importer |
| `data/employees.csv` | Sample input (the last row is invalid on purpose) |
| `docs/week1` | Daily notes |

## Run

```bash
dotnet build
dotnet run --project src/EmployeeManagement.ConsoleApp -- <csv-file> <command> [argument]
```

Commands: `list`, `summary`, `search <text>`, `dept <name>`, `raise <percent>`.
`dotnet run --project src/EmployeeManagement.ConsoleApp -- linq` runs the Day 3 LINQ exercises against built-in sample data.
`dotnet run --project src/EmployeeManagement.ConsoleApp -- import <csv-file>` imports asynchronously. Ctrl+C cancels it.

## Test

```bash
dotnet test
```
Invalid rows are skipped and reported on stderr. A missing file exits with code `2`, and bad arguments exit with code `1`.

## Sample inputs and outputs

**1. Summary**

```text
> dotnet run --project src/EmployeeManagement.ConsoleApp -- data/employees.csv summary
Skipped Line 10: 'not-a-number' is not a valid salary.
Employees : 8
Total     : 864,000.00
Average   : 108,000.00
Min / Max : 64,000.00 / 185,000.00
By department:
  Engineering  3
  HR           1
  Sales        2
  Support      2
```

**2. Department filter**

```text
> dotnet run --project src/EmployeeManagement.ConsoleApp -- data/employees.csv dept sales
Skipped Line 10: 'not-a-number' is not a valid salary.
Id   Name                 Department         Salary Hired
--------------------------------------------------------------
4    Rahul Menon          Sales           76,000.00 2021-11-01
6    Arjun Pillai         Sales          121,000.00 2017-09-25
2 employee(s)
```

**3. Error handling (missing file)**

```text
> dotnet run --project src/EmployeeManagement.ConsoleApp -- missing.csv list
Input file 'missing.csv' was not found.
(exit code 2)
```

## Daily notes

- Day 1: [Value vs reference types](docs/week1/day1-value-vs-reference-types.md)
- Day 2: [Domain model, class diagram and SOLID](docs/week1/day2-domain-design.md)
- Day 3: [LINQ queries with inputs, outputs and complexity](docs/week1/day3-linq-queries.md)
- Day 4: [Testing, async and tooling](docs/week1/day4-testing-and-async.md) and the [build/test log](docs/week1/day4-build-test-log.txt)
