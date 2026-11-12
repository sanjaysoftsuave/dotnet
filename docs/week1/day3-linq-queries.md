# Day 3: LINQ queries

- Query file: [`EmployeeQueries.cs`](../../src/EmployeeManagement.Domain/Queries/EmployeeQueries.cs)
- Run all queries: `dotnet run --project src/EmployeeManagement.ConsoleApp -- linq`

**Input** for every query is `SampleData.Create()`: 4 departments and 10 employees (8 full-time, 2 contractors).

| Id | Name | Dept | Kind | Hired | Annual cost | Manager |
|---|---|---|---|---|---|---|
| 1 | Asha Raman | Engineering | Full-time | 2018-04-02 | 222,000 | |
| 2 | Vikram Iyer | Engineering | Full-time | 2020-07-15 | 170,400 | 1 |
| 3 | Meera Nair (no email) | Engineering | Full-time | 2023-01-09 | 117,600 | 2 |
| 4 | Rahul Menon | Sales | Full-time | 2021-11-01 | 91,200 | 6 |
| 5 | Divya Krishnan | HR | Full-time | 2019-03-18 | 105,600 | |
| 6 | Arjun Pillai | Sales | Full-time | 2017-09-25 | 145,200 | |
| 7 | Kavya Suresh | Support | Full-time | 2024-02-12 | 76,800 | 8 |
| 8 | Nikhil Das | Support | Full-time | 2016-06-30 | 108,000 | |
| 9 | Farhan Ali | Engineering | Contractor (to 2026-12-31) | 2025-01-06 | 120,000 | 1 |
| 10 | Lena Joseph (no email) | Support | Contractor (to 2025-06-30) | 2024-06-03 | 48,000 | 8 |

Full-time cost is salary × 1.20 (benefits). Contractor cost is hourly rate × hours per year.

In the table below, *n* = employees, *d* = departments, *k* = items requested and *p* = predicates.

| # | Query | LINQ operators | Output | Time | Extra space |
|---|---|---|---|---|---|
| 1 | `SearchByName("an")` | `Where` | Asha Raman, Divya Krishnan, Farhan Ali | O(n·m), where m is name length | O(1), streamed |
| 2 | `HiredBetween(2019-01-01, 2021-12-31)` | `Where` | Vikram Iyer, Rahul Menon, Divya Krishnan | O(n) | O(1), streamed |
| 3 | `OrderBySeniority().Take(3)` | `OrderBy`, `ThenBy` | Nikhil Das, Arjun Pillai, Asha Raman | O(n log n) | O(n), the sort buffers everything |
| 4 | `TopByCost(3)` | `OrderByDescending`, `ThenBy`, `Take` | Asha 222,000, Vikram 170,400, Arjun 145,200 | O(n log n) | O(n) |
| 5 | `CostByDepartment(departments)` | `GroupJoin`, `Count`, `Sum`, `OrderByDescending` | Engineering 630,000 / 500,000 **over budget**, Sales 236,400 / 250,000, Support 232,800 / 150,000 **over budget**, HR 105,600 / 120,000 | O(n + d log d) | O(n + d), lookup by department |
| 6 | `CountByEmploymentType()` | `GroupBy`, `ToDictionary` | FullTime 8, Contractor 2 | O(n) | O(n) |
| 7 | `GroupBySalaryBand()` | `GroupBy` on a pattern-matching key, `OrderBy` | Junior: Rahul, Divya, Kavya; Mid: Vikram, Meera, Arjun, Nikhil; Senior: Asha; Contract: Farhan, Lena | O(n) | O(n) |
| 8 | `ActiveContractorsOn(2026-01-01)` | `OfType`, `Where` | Farhan Ali | O(n) | O(1), streamed |
| 9 | `ManagersWithReports()` | `Where`, `GroupBy`, `Join`, `OrderByDescending` | Asha 2, Nikhil 2, Arjun 1, Vikram 1 | O(n log n) | O(n) |
| 10 | `MatchingAll(InDepartment(1), CostsAtLeast(150000))` | `Where`, `All` over `Func<Employee,bool>` delegates | Asha Raman, Vikram Iyer | O(n·p) | O(1), streamed |
| 11 | `AnyWithoutEmail()` | `Any` (short-circuits) | `True` (stops at Meera, the 3rd item) | O(n) worst case | O(1) |
| 12 | `Page(2, 4)` (generic) | `Skip`, `Take` | Divya Krishnan, Arjun Pillai, Kavya Suresh, Nikhil Das | O(page × size) | O(1), streamed |

## Concepts covered

- **Extension methods.** Every query is a `static` method that takes `this IEnumerable<Employee>`, so it reads like a built-in operator: `staff.TopByCost(3)`.
- **Deferred execution.** Queries 1–5 and 7–10 and 12 return `IEnumerable<T>` and do nothing until they are enumerated. Each enumeration runs the query again against the *current* data. Query 6 (`ToDictionary`) and query 11 (`Any`) execute immediately.
- **Eager guard clauses.** `HiredBetween` and `Page` are ordinary methods, not iterators with `yield`, so bad arguments throw when you *call* them, not later when you enumerate.
- **Generics.** `Page<T>` works for any sequence. `InMemoryRepository<T> where T : Entity` holds the storage code shared by both repositories.
- **Lambdas and delegates.** `EmployeeFilters` returns `Func<Employee, bool>` closures that `MatchingAll` combines.
- **Events.** `OrganizationService.EmployeeTransferred` (`EventHandler<EmployeeTransferredEventArgs>`) fires after a real transfer. Subscribers such as an audit log need no changes to the service.
- **Pattern matching.** `SalaryBands.Classify` uses type and property patterns (`FullTimeEmployee { AnnualSalary: >= 150_000 }`). The console app uses a list pattern (`args is ["linq"]`).

## `List<T>` vs `Dictionary<TKey,TValue>`

The repositories store items in a `Dictionary<int, T>`, so `GetById` is O(1). A `List<T>` would need an O(n) scan to find an item by id. A `List<T>` keeps insertion order and is cheap to iterate, so query results are returned as lists.

## `IEnumerable<T>` vs `IQueryable<T>`

These queries run in memory over `IEnumerable<T>`, and the lambdas are compiled delegates. In Week 2, EF Core exposes `IQueryable<T>`. The same lambdas become *expression trees* that EF Core translates to SQL, so filtering happens in the database.
