# Day 10: API security (Gate 2)

## Authentication vs authorization

- **Authentication** answers *who are you?* The JWT bearer handler checks the token's signature, issuer, audience and expiry, then builds a `ClaimsPrincipal` (the user) from its claims. It runs in `UseAuthentication()`.
- **Authorization** answers *are you allowed to do this?* Policies on each endpoint are checked against the user's claims. It runs in `UseAuthorization()`, *after* authentication.

## 401 vs 403

| Status | Meaning | When this API returns it |
|---|---|---|
| **401 Unauthorized** | "I don't know who you are." The response includes `WWW-Authenticate: Bearer`. | No token, a tampered or wrongly signed token, an expired token, or the wrong issuer or audience |
| **403 Forbidden** | "I know who you are, and you can't do this." Sending the same token again will not help. | A Viewer tries to create an employee. An HrManager tries to create a department. |

Both come back as `ProblemDetails`, because `UseStatusCodePages()` fills the empty body the auth handlers leave.

## What is a claim?

A claim is a statement about the user, issued and signed by a trusted party. It is a name/value pair inside the token. This API's tokens contain:

```json
{ "sub": "anita", "name": "anita", "jti": "4f1c…", "role": "HrAdmin",
  "iss": "EmployeeManagement.Api", "aud": "EmployeeManagement.Clients", "nbf": …, "exp": …, "iat": … }
```

The API trusts these values only because the signature (HMAC-SHA256 with the server's secret key) verifies. Change one character and the token is rejected with 401.

## Roles and policies

Controllers ask for a **capability** (policy), not a role, so the mapping lives in one place ([`Security/Authorization.cs`](../../src/EmployeeManagement.Api/Security/Authorization.cs)):

| Policy | Viewer | HrManager | HrAdmin | Applied to |
|---|:-:|:-:|:-:|---|
| `CanReadData` | ✅ | ✅ | ✅ | every endpoint on both controllers (class level) |
| `CanManageEmployees` | ❌ | ✅ | ✅ | `POST/PUT/DELETE /api/employees…`, `POST /raise` |
| `CanManageDepartments` | ❌ | ❌ | ✅ | `POST/PUT/DELETE /api/departments…` |

A **fallback policy** requires an authenticated user for any endpoint that has no `[Authorize]` or `[AllowAnonymous]`, so a new endpoint is protected by default. Only `POST /api/auth/token` and the OpenAPI document are anonymous.

## Getting a token

`POST /api/auth/token` with `{ "userName": "anita", "role": "HrAdmin" }` returns `{ accessToken, tokenType, expiresAt }`. **This endpoint works in Development only and returns 404 elsewhere.** It skips passwords so the scenarios can be demonstrated. In production, tokens would come from an identity provider (Entra ID, Auth0, Keycloak) using OAuth 2.0 / OpenID Connect. The API would only validate them, typically using `Authority` instead of a shared key.

In Swagger UI (`/swagger`), call the token endpoint, click **Authorize** and paste the token. The `.http` file captures the tokens automatically.

## Configuration and secrets

| Setting | Where | Secret? |
|---|---|---|
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes` | `appsettings.json` | No |
| `Jwt:SigningKey` (at least 32 characters) | **user-secrets** locally, an **environment variable** or key vault on servers | **Yes, never committed** |

```bash
# one-time local setup (stored in your user profile, outside the repository)
dotnet user-secrets set "Jwt:SigningKey" "<at least 32 random characters>" --project src/EmployeeManagement.Api

# servers / CI: environment variable (double underscore = section separator)
Jwt__SigningKey=<value from your secret store>
```

`JwtOptions` is validated at startup (`ValidateOnStart`). With no key, the app refuses to start with:

```text
OptionsValidationException: DataAnnotation validation failed for 'JwtOptions' members: 'SigningKey' with the error:
'Jwt:SigningKey is not configured. See README > Authentication for how to set it.'
```

There is no default key in code, so a deployment can never fall back to a known value. Other hardening:

- `MapInboundClaims = false`, so claim names stay as issued.
- A 30-second clock skew instead of the 5-minute default.
- **HSTS and HTTPS redirection** outside Development, because bearer tokens must not travel over plain HTTP.

## Demo evidence

Captured from the running API with the key supplied as an environment variable:

```text
GET  /api/employees/1     (no token)        -> 401  {"title":"Unauthorized","status":401,...}   + WWW-Authenticate: Bearer
GET  /api/employees/1     (Viewer)          -> 200  {"id":1,"firstName":"Asha",...}
POST /api/employees       (Viewer)          -> 403  {"title":"Forbidden","status":403,...}
POST /api/employees       (HrManager)       -> 201  {"id":12,"firstName":"Priya",...}
POST /api/departments     (HrManager)       -> 403  {"title":"Forbidden","status":403,...}
POST /api/departments     (HrAdmin)         -> 201  {"id":5,"name":"Finance","code":"FIN",...}
GET  /api/employees/1     (tampered token)  -> 401
```

## Tests added

- `Security/AuthorizationPolicyTests` (unit): the full role × policy matrix (9 cases), an unknown role, and the fallback policy rejecting anonymous users.
- `Security/JwtTokenServiceTests` (unit): claims, issuer and expiry using a fake clock; a token validates with the right key and fails with another; a missing or short key fails options validation.
- `Integration/SecurityEndpointTests` (`WebApplicationFactory<Program>`, real pipeline, throwaway SQLite file): 401 without a token, 200 for Viewer reads, 403 for Viewer writes, 201 for HrManager creates, 403 for HrManager department changes and 201 for HrAdmin, 401 for a forged signature, 401 for an expired token, the demo token round trip, 400 for an unknown role, and an anonymous OpenAPI document with the bearer scheme.

## Gate 2 checklist

| Requirement | Where |
|---|---|
| Layered endpoints | `Api` → `Application` → `Domain`, with `Infrastructure` implementing persistence ([Day 8](day8-layered-architecture.md)) |
| Database persistence | EF Core + SQLite, migrations, relationships, filtering, sorting and paging ([Day 9](day9-ef-core.md)) |
| Validation | DTO attributes + `IValidatableObject` + service rules → `ValidationProblemDetails` |
| Error handling | `GlobalExceptionHandler` + `UseStatusCodePages`, consistent `ProblemDetails` with a `correlationId` |
| Authorization | JWT bearer, role-based policies, fallback policy (this page) |
| Tests | 142 passing across Domain, Application, Infrastructure (real SQLite) and Api (unit + integration). See [build-test-log.txt](build-test-log.txt). |
| Documentation | OpenAPI ([openapi-v1.json](openapi-v1.json)), Swagger UI, [`.http` requests](../../src/EmployeeManagement.Api/EmployeeManagement.Api.http), README |
