using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EmployeeManagement.Api;

// Adds the "Bearer" scheme to the OpenAPI document so Swagger UI shows an Authorize button.
internal sealed class OpenApiBearerTransformer : IOpenApiDocumentTransformer
{
    private const string SchemeName = "Bearer";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste a token from POST /api/auth/token (Development only).",
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(SchemeName, document)] = [] });
        return Task.CompletedTask;
    }
}
