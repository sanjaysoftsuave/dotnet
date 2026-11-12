# Day 4: Unit testing, async and tooling

## Solution layout

```text
EmployeeManagement.sln
├── src/EmployeeManagement.ConsoleApp       app (exe)
├── src/EmployeeManagement.Domain           class library
└── tests/EmployeeManagement.Domain.Tests   xUnit tests → references Domain
```

## Testing approach

- Every test follows **Arrange-Act-Assert** and is named `Method_Scenario_ExpectedResult`.
- Tests are **deterministic**. There is no clock, network or file system: dates are fixed, CSV input comes from a `StringReader`, and data comes from `SampleData.Create()`.
- **Boundaries.** Tests cover domain rules (entities, `OrganizationService`), LINQ queries and the importer. The console app is a thin shell and is checked by running it.
- xUnit creates a **new class instance per test**, so the constructor gives each test fresh repositories with no shared state.

Full output: [day4-build-test-log.txt](day4-build-test-log.txt), 19 passing.

## Async operation with cancellation

`EmployeeCsvImporter.ImportAsync(TextReader, CancellationToken)`:

- Awaits `ReadLineAsync(cancellationToken)`, so the thread is not blocked while waiting on I/O.
- Calls `ThrowIfCancellationRequested()` between rows and lets `OperationCanceledException` propagate. It never swallows cancellation.
- The console `import` command creates a `CancellationTokenSource` that cancels on **Ctrl+C** or after a **30-second timeout**. It catches `OperationCanceledException` and exits with code `3`.
- `ImportAsync_CancelledToken_ThrowsAndImportsNothing` proves the behaviour.

```bash
dotnet run --project src/EmployeeManagement.ConsoleApp -- import data/employees.csv
```

### `Task` vs `Thread`

A `Thread` is an OS thread that you own. It is expensive to create and blocks while it waits. A `Task` represents a unit of work or a future result. It usually runs on the thread pool, and for I/O it may use no thread at all while waiting. `async`/`await` composes tasks without blocking.

## CLI commands

| Command | What it does |
|---|---|
| `dotnet restore` | Downloads the NuGet packages listed in each `.csproj` and writes `obj/project.assets.json` |
| `dotnet build` | Restores, then compiles every project in the solution into `bin/` |
| `dotnet test` | Builds, then discovers and runs the tests with the xUnit adapter |
| `dotnet add <proj> reference <proj>` | Adds a `<ProjectReference>` |
| `dotnet add <proj> package <name>` | Adds a NuGet `<PackageReference>` |
