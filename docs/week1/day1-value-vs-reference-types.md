# Day 1: Value vs reference types in the console app

> This note describes the Day 1 code (commit "Day 1: C# fundamentals console app"). On Day 5, `EmployeeRecord` was replaced by the domain `Employee` classes and `SalaryStats` became `CostStatistics` (a `readonly record struct`). The same value/reference rules apply.

## Value types used

| Type | Where | Why it matters |
|---|---|---|
| `int` | `EmployeeRecord.Id` | Copied on assignment. Two variables never share it. |
| `decimal` | `EmployeeRecord.Salary` | Exact base-10 arithmetic for money, unlike `double`. `CalculateRaisedSalary(decimal salary, ...)` gets a copy, so the caller's variable is never changed. |
| `DateOnly` | `EmployeeRecord.HireDate` | A struct, so it is copied like `int`. |
| `int?` (`Nullable<int>`) | `EmployeeRecord.ManagerId` | A value type that can also be "no value". Top-level managers have `null`. |
| `SalaryStats` (`readonly struct`) | `EmployeeProcessor.CalculateStats` | A small, immutable result that is returned by value. `readonly` stops callers from changing it, and it lives inline rather than as a separate heap object. |

## Reference types used

| Type | Where | Why it matters |
|---|---|---|
| `EmployeeRecord` (class) | Every list | The list stores references. `ApplyRaise` changes `Salary` on the same objects the list holds, so the table printed afterwards shows the new salaries. |
| `string` | Names, department | A reference type that behaves like a value because it is immutable. Every change creates a new string. |
| `string?` | `EmployeeRecord.Email` | Nullable reference type annotation. The compiler warns if we use it without a null check (`Email?.Contains(...) ?? false`). |
| `string[]`, `List<T>`, `Dictionary<K,V>` | Parser and processor | Arrays and collections are always reference types, even when they hold value types. |
| `StringBuilder` | `ReportFormatter` | Builds the report in one mutable buffer. Using `+=` on a `string` in a loop would allocate a new string on every iteration. |

## Quick demonstration

```csharp
decimal salary = 100m;
decimal raised = EmployeeProcessor.CalculateRaisedSalary(salary, 10); // salary is still 100

var employee = new EmployeeRecord { Salary = 100m };
var sameEmployee = employee;          // copies the reference, not the object
sameEmployee.Salary = 200m;           // employee.Salary is now 200 too
```
