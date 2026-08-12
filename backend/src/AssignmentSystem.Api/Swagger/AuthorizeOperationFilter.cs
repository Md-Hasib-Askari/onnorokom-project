using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AssignmentSystem.Api.Swagger;

/// <summary>
/// Adds the bearer-token security requirement to every endpoint in the generated OpenAPI doc,
/// so "Authorize" in Swagger UI applies to all routes instead of just those with a manual
/// <c>Security</c> attribute.
/// </summary>
public class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                [new OpenApiSecuritySchemeReference("Bearer", null, null)] = new List<string>(),
            },
        };
    }
}
