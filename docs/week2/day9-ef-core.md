# Day 9: EF Core and database integration

## Database setup

The API uses **SQLite** through EF Core 10. It is a real relational database stored in one file (`src/EmployeeManagement.Api/employees.db`), so there is no server to install. SQL Server or PostgreSQL works by swapping `UseSqlite` for `UseSqlServer` or `UseNpgsql` and regenerating the migration.

**Development:** nothing to do. `appsettings.Development.json` sets `Database:MigrateOnStartup` and `Database:SeedSampleData` to `true`, so the first `dotnet run` creates the schema and loads the sample organisation.

**Other environments:** both settings default to `false`. Apply migrations explicitly:

```bash
dotnet tool restore                                    # installs dotnet-ef from dotnet-tools.json
dotnet ef database update -p src/EmployeeManagement.Infrastructure -s src/EmployeeManagement.Api
```

**Add a migration after changing the model:**

```bash
dotnet ef migrations add <Name> -p src/EmployeeManagement.Infrastructure -s src/EmployeeManagement.Api -o Persistence/Migrations
```

**Reset the local database:** delete `src/EmployeeManagement.Api/employees.db` and run the API again. `*.db` files are git-ignored.

## Model

| Table | Notes |
|---|---|
| `Departments` | Unique indexes on `Name` and `Code` |
| `Employees` | **Table-per-hierarchy:** full-time employees and contractors share one table, and the `EmploymentType` discriminator column decides which C# class is created. The owned `Address` record becomes the `Address_*` columns. |

### Relationships

There are two foreign keys, both configured with the Fluent API in `Persistence/Configurations`:

- `Employees.DepartmentId → Departments.Id`, **many-to-one**.
- `Employees.ManagerId → Employees.Id`, a **self-reference**: an employee's manager is another employee.

Both use `ON DELETE RESTRICT`. The services already return 409 for "department still has staff" and "employee still manages people", and the database enforces the same rule if anything bypasses the service (test `DeleteDepartmentWithStaff_IsBlockedByForeignKey`).

The domain classes hold ids rather than navigation properties, so EF Core is mapped onto the existing model without changing it. Read-only properties (`Department.Code`, `Employee.HireDate`) are mapped explicitly, and EF writes their backing fields. Entities are created through their constructors, so domain validation still runs when rows are loaded.

## Filtering, sorting and pagination

`GET /api/employees` takes these query parameters: `departmentId`, `employmentType`, `search` (name or email, case-insensitive), `hiredFrom`, `hiredTo`, `sortBy` (`Id`, `LastName`, `HireDate`), `sortDirection`, `page`, and `pageSize` (1–100).

The rules live in `Application/Employees/EmployeeQueryFilters.cs` as plain LINQ over `IQueryable<Employee>`:

- **EF Core** translates them to SQL in `EfEmployeeRepository`.
- **Unit tests** run the same rules over an in-memory list with `AsQueryable()` (`EmployeeQueryFiltersTests`).

The response is a `PagedResult<T>` with `items`, `page`, `pageSize`, `totalCount`, `totalPages` and `hasNextPage`.

### Generated SQL

`GET /api/employees?departmentId=1&search=a&sortBy=HireDate&sortDirection=Desc&page=1&pageSize=2` logged two commands (from Development logging, with `Microsoft.EntityFrameworkCore.Database.Command` set to Information):

```sql
SELECT COUNT(*) FROM "Employees" AS "e"
WHERE "e"."DepartmentId" = @departmentId
  AND (instr(lower("e"."FirstName" || ' ' || "e"."LastName"), @term) > 0
       OR ("e"."Email" IS NOT NULL AND instr(lower("e"."Email"), @term) > 0))

SELECT "e"."Id", "e"."DepartmentId", "e"."Email", "e"."FirstName", "e"."HireDate", "e"."EmploymentType", "e"."LastName",
       "e"."ManagerId", "e"."ContractEnd", "e"."HourlyRate", "e"."HoursPerYear", "e"."AnnualSalary", "e"."BenefitsRate",
       "e"."Address_City", "e"."Address_Country", "e"."Address_PostalCode", "e"."Address_Street"
FROM "Employees" AS "e"
WHERE "e"."DepartmentId" = @departmentId AND (instr(lower(...), @term) > 0 OR (...))
ORDER BY "e"."HireDate" DESC, "e"."Id"
LIMIT @p4 OFFSET @p
```

- The filter, sort and page all run **in the database**. Only one page of rows is sent to the API.
- Values are **parameters** (`@departmentId`, `@term`), never concatenated into the SQL string, so the query is safe from SQL injection.
- `ThenBy(Id)` gives a stable order, so rows with the same hire date never move between pages.
- `EfEmployeeRepositoryTests.ListQuery_GeneratedSql_FiltersAndPagesInTheDatabase` checks this with `ToQueryString()`.

The department cost summary (`GET /api/departments/{id}/cost`) loads that department's rows and totals them in C#. The reason is that `CalculateAnnualCost()` is a C# method (salary × benefits, or rate × hours), which EF cannot translate. SQLite also stores `decimal` as TEXT, so SQL `SUM` would be wrong. With SQL Server this could become a `GROUP BY` projection.

## Tracking vs AsNoTracking

| | Tracked (default) | `AsNoTracking()` |
|---|---|---|
| What EF keeps | A snapshot of every loaded entity in the `DbContext` | Nothing |
| `SaveChanges` | Detects changes and issues `UPDATE`s | Ignores changes to these objects |
| Cost | More memory and CPU per entity | Cheaper, the right choice for read-only queries |
| Used here for | `GetByIdAsync` (the service changes the entity, then saves) | lists, existence checks, cost summary |

`TrackedEntity_ChangeIsSaved_NoTrackingEntity_ChangeIsIgnored` gives a raise to both kinds of entity: only the tracked one is persisted.

## Loading strategies

The model has no navigation properties, so related data is loaded explicitly with ids or joined in a query. With navigations, the options are:

- **Eager loading:** `Include(e => e.Department)` gets everything in one query with a JOIN. Use it when you know you need the related data.
- **Explicit loading:** `db.Entry(e).Reference(x => x.Department).LoadAsync()`. Use it when you only sometimes need the related data.
- **Lazy loading:** a proxy loads the related data the first time the property is touched. It is convenient, but it causes the **N+1 problem**.

### The N+1 problem

N+1 means one query for the list and then one more query *per row* for its related data. Listing 100 employees and touching `employee.Department.Name` lazily runs 101 queries. Fixes:

- `Include` the related data (one JOIN).
- Project only the needed columns (`Select(e => new { e.FirstName, Department = e.Department.Name })`).
- Batch the lookups by id.

This API avoids it: list endpoints never fetch per-row data, and the "has staff?" and "is a manager?" checks are single `EXISTS` queries.

## Transactions and DbContext lifetime

- `DbContext` is **scoped**, one per request. It is not thread-safe, so it is never a singleton. It also acts as the `IUnitOfWork`.
- A single `SaveChangesAsync` is **one transaction**. A service makes several changes (rename, transfer and new manager in `UpdateAsync`) and they commit or roll back together.
- Use `db.Database.BeginTransactionAsync()` only when one use case needs several `SaveChanges` calls, or raw SQL plus EF changes, to be atomic.

## EF Core vs Dapper

| | EF Core | Dapper |
|---|---|---|
| Writing queries | LINQ, translated to SQL | Hand-written SQL |
| Change tracking, migrations, relationships | Built in | None. You write the INSERT and UPDATE statements and manage the schema yourself. |
| Performance | Very good, with some overhead for tracking and translation | Closest to raw ADO.NET |
| Best for | Domain models with CRUD and business rules (this API) | Read-heavy reporting, complex hand-tuned SQL |

Many systems use both: EF Core for writes, Dapper for heavy read queries.

## Tests added

- `tests/EmployeeManagement.Infrastructure.Tests` (new) runs against **real SQLite in memory** with the real migrations:
  - filter + sort + page with TPH materialisation;
  - case-insensitive search translated to SQL;
  - generated SQL contains WHERE, ORDER BY and LIMIT/OFFSET;
  - tracked vs no-tracking changes;
  - the foreign key blocks deleting a department that has staff.
- `EmployeeQueryFiltersTests` (Application.Tests): every filter and sort rule, inclusive date range, paging, query validation and `PagedResult` maths, all without a database.
