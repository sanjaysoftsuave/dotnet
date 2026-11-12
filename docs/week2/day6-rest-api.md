# Day 6: ASP.NET Core and REST basics

- Project: `src/EmployeeManagement.Api` (controller-based Web API)
- Requests: [`EmployeeManagement.Api.http`](../../src/EmployeeManagement.Api/EmployeeManagement.Api.http). Open it in VS Code (REST Client extension) or Visual Studio. It covers success and failure cases.
- OpenAPI: `GET /openapi/v1.json` and Swagger UI at `/swagger` (Development only). A saved copy is in [openapi-v1.json](openapi-v1.json).

```bash
dotnet run --project src/EmployeeManagement.Api --launch-profile http
# http://localhost:5006/swagger
```

## Endpoints and status codes

| Method | Route | Success | Failure |
|---|---|---|---|
| GET | `/api/employees` | 200 | |
| GET | `/api/employees/{id}` | 200 | 404 unknown id |
| POST | `/api/employees` | 201 + `Location` header | 400 validation or business rule (unknown department or manager) |
| PUT | `/api/employees/{id}` | 204 | 400, 404 |
| POST | `/api/employees/{id}/raise` | 200 with the updated employee | 400 (percent outside 0.01–50, or the employee is a contractor), 404 |
| DELETE | `/api/employees/{id}` | 204 | 404 (second delete of the same id) |
| GET | `/api/departments` | 200 | |
| GET | `/api/departments/{id}` | 200 | 404 |
| GET | `/api/departments/{id}/cost` | 200 | 404 |
| POST | `/api/departments` | 201 | 400, 409 duplicate name |
| PUT | `/api/departments/{id}` | 204 | 400, 404 |
| DELETE | `/api/departments/{id}` | 204 | 404, 409 department still has staff |

Every error body is `application/problem+json` (RFC 9457 `ProblemDetails`). Validation errors use `ValidationProblemDetails` with an `errors` dictionary.

### When to return which code

- **200 OK.** A read, or an action that returns the changed resource (the raise).
- **201 Created.** A new resource. `CreatedAtAction` adds a `Location: /api/employees/{id}` header and returns the resource.
- **204 No Content.** It succeeded and there is nothing to return (PUT, DELETE).
- **400 Bad Request.** The client sent something invalid: a malformed body, a failed validation rule, or a broken business rule.
- **404 Not Found.** The resource in the URL does not exist.
- **409 Conflict.** The request is valid, but it clashes with the current state (duplicate name, or deleting a department that still has staff).

## What happens when a request reaches the API

1. **Kestrel** accepts the connection and parses HTTP into an `HttpContext`.
2. The **middleware pipeline** runs in order (routing, plus later auth and logging). Each middleware can short-circuit or call `next`.
3. **Routing** matches `POST /api/employees` to `EmployeesController.Create` using the attribute routes.
4. **Model binding** builds `CreateEmployeeRequest` from the JSON body with `System.Text.Json`. Enums bind from strings because of `JsonStringEnumConverter`.
5. **Validation.** `[ApiController]` runs the DataAnnotations and `IValidatableObject.Validate`. If the model is invalid, it returns 400 `ValidationProblemDetails` and the action never runs.
6. The **action** runs: it maps the DTO to a domain entity, applies business rules and stores the entity.
7. The **result** (`CreatedAtAction`, `NotFound`, `Problem`…) is executed. The response is serialised to JSON and flows back out through the middleware.

## PUT vs PATCH

- **PUT** replaces the resource's editable state. The client sends *every* editable field, and anything left out is reset (here `Email: null` clears the email). Sending the same PUT twice gives the same result, so PUT is **idempotent**.
- **PATCH** sends only the fields to change (JSON Merge Patch or JSON Patch). It is smaller and avoids overwriting concurrent edits to other fields, but "missing" vs "set to null" has to be handled carefully, and PATCH is not guaranteed to be idempotent.
- This API uses PUT for edits. Changes with business meaning get their own action endpoint (`POST /raise`), because that makes the rule explicit and avoids letting clients set a salary to any value.

## DTOs

Requests and responses (`Contracts/`) are separate from domain entities. That keeps the following out of the public contract: private-setter invariants, the polymorphic class hierarchy, and fields clients must not set (`Id`, `AnnualCost`). Mapping is manual (`Mapping/ContractMappings.cs`).

## Tests

`tests/EmployeeManagement.Api.Tests` checks the extracted validation rules (attributes plus cross-field `Validate`) and the mapping. The test helper runs the same `Validator` that MVC uses.

## Known limitations (fixed on later days)

- Storage is in-memory and resets on restart. It is not thread-safe under concurrent writes (Day 9: EF Core).
- Error handling is repeated in each controller with `try/catch` (Day 8: central exception handler).
- No authentication (Day 10).
