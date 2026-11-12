# Day 5: Gate 1 mini-project walkthrough

## What changed on Day 5

- **Refactor for clarity and testability.** The console app no longer has its own record type, CSV parser and loop-based processor from Day 1. Every command now loads data through `EmployeeCsvImporter` into the domain model and answers with `EmployeeQueries`. The console is a thin shell made of three parts: `Program` (arguments, exit codes and cancellation), `ReportCommands` (command dispatch) and `ReportFormatter` (text output). All the business logic lives in the tested Domain library.
- **New command:** `hired <from> <to>`.
- **New query:** `SummarizeCost()`, which uses `Aggregate` to compute a `CostStatistics` `readonly record struct` in one pass.
- **Tests** went from 19 to 53. They cover normal, boundary and invalid inputs, plus a regression test for an injected defect.

## Defect DEF-001 and its regression test

| | |
|---|---|
| **Symptom** | `hired 2021-06-01 2021-11-01` did not list Rahul Menon, who was hired on 2021-11-01. The method's own comment says the range is inclusive. |
| **Root cause** | [`EmployeeQueries.HiredBetween`](../../src/EmployeeManagement.Domain/Queries/EmployeeQueries.cs) compared the end date with `<` instead of `<=`. The defect was injected in the Day 3 commit. |
| **Fix** | `e.HireDate <= to` |
| **Regression test** | `HiredBetweenTests.HiredBetween_EmployeeHiredOnEndDate_IsIncluded` |

Evidence. Running the tests against the defective code:

```text
Failed HiredBetweenTests.HiredBetween_SameStartAndEndDate_ReturnsHiresOnThatDay
Failed HiredBetweenTests.HiredBetween_EmployeeHiredOnEndDate_IsIncluded
Failed!  - Failed: 2, Passed: 3, Total: 5
```

After the fix:

```text
Passed!  - Failed: 0, Passed: 53, Total: 53
```

## How the test cases were chosen

| Kind | Examples |
|---|---|
| Normal | A valid CSV row parses into every field. `HiredBetween` over a wide range. `TopByCost(3)`. |
| Boundary | Hire date equal to `from`, equal to `to`, and `from == to`. The last partial page and a page beyond the end. A raise of exactly 50% and 0.01%. A salary of `0`. A contractor on the last day of the contract. An empty employee list. |
| Invalid | Wrong column count, non-numeric or negative values, an impossible date (`2023-02-30`), `from > to`, page `0`, a raise over 50%, department codes with symbols or more than 10 characters. |

Each rule has a test just inside its limit and one just outside, because that is where off-by-one mistakes like DEF-001 hide.

## Walkthrough: one class

[`FullTimeEmployee`](../../src/EmployeeManagement.Domain/Employees/FullTimeEmployee.cs) derives from the abstract `Employee`:

- The constructor validates input (salary not negative, benefits rate between 0 and 1) and passes the shared fields to the `protected` base constructor. An invalid object can never exist.
- `AnnualSalary` has a `private set`. The only way to change it is `GiveRaise`, which enforces the rule that a raise is greater than 0% and at most 50%.
- It overrides the abstract `CalculateAnnualCost()`. `OrganizationService` and every LINQ query call the base method and never check which subclass they have. That is polymorphism and the Liskov substitution principle.
- It is `sealed` because nothing should change its pricing rule by subclassing it.

## Walkthrough: one LINQ query

`CostByDepartment` uses `GroupJoin`, `Count`, `Sum` and `OrderByDescending`:

```csharp
departments
    .GroupJoin(employees, d => d.Id, e => e.DepartmentId,
        (d, staff) => new DepartmentCostSummary(d.Name, staff.Count(), staff.Sum(e => e.CalculateAnnualCost()), d.AnnualBudget))
    .OrderByDescending(s => s.TotalCost);
```

- `GroupJoin` is a left join: a department with no staff still appears, with a count of 0. A plain `Join` would drop it.
- On first enumeration, LINQ builds a lookup of employees by `DepartmentId` (O(n)). Each department then finds its staff in O(1), so the whole query is O(n + d log d).
- The query is **deferred**. Calling the method costs nothing until someone enumerates it. Each enumeration runs it again, so it always reflects the current data. Materialise it with `ToList()` if you need to enumerate it several times.
- **Pitfall:** `staff` is enumerated twice (`Count` and `Sum`). That is fine here because `GroupJoin` hands over an in-memory group. Over a lazy source it would do the work twice.

## Walkthrough: one async operation

[`EmployeeCsvImporter.ImportAsync`](../../src/EmployeeManagement.Domain/Import/EmployeeCsvImporter.cs):

- It awaits `ReadLineAsync(cancellationToken)` line by line, so no thread is blocked during file I/O.
- It checks the token between rows, so Ctrl+C or the 30-second timeout in `Program` stops the import. The resulting `OperationCanceledException` reaches `Program`, which exits with code 3. A cancelled import is never reported as a success.
- Bad rows are collected as errors rather than thrown, so one typo does not abort the whole file.
- It has two passes, because a manager may appear later in the file than their reports.
- **Pitfalls avoided:** no `.Result` or `.Wait()` (they can deadlock and they wrap exceptions in `AggregateException`), no `async void`, and cancellation is never swallowed by a catch-all.

## One SOLID principle in this code: dependency inversion

`OrganizationService` and `EmployeeCsvImporter` take `IEmployeeRepository` and `IDepartmentRepository` through their constructors. They never create storage themselves. Tests pass the in-memory implementations, and in Week 2 the API will pass EF Core implementations without changing either class.
