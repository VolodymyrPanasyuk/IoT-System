using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace IoT_System.Infrastructure.Swagger;

/// <summary>
/// Swagger operation filter that adds security requirements only to endpoints with [Authorize] attribute.
/// Respects attribute inheritance and method-level overrides.
/// </summary>
public class SwaggerAuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check method-level attributes first (they have priority)
        var methodAllowAnonymous = context.MethodInfo.GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>().Any();

        var methodAuthorize = context.MethodInfo.GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>().Any();

        // If method has explicit [AllowAnonymous], skip security
        if (methodAllowAnonymous)
        {
            return;
        }

        // If method has explicit [Authorize], add security
        if (methodAuthorize)
        {
            AddSecurityRequirement(operation);
            return;
        }

        // Check controller-level attributes
        var controllerAllowAnonymous = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>().Any() ?? false;

        var controllerAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>().Any() ?? false;

        // Controller has [AllowAnonymous] and method doesn't override it
        if (controllerAllowAnonymous)
        {
            return;
        }

        // Controller has [Authorize] and method doesn't override it
        if (controllerAuthorize)
        {
            AddSecurityRequirement(operation);
        }
    }

    private void AddSecurityRequirement(OpenApiOperation operation)
    {
        // Add 401 and 403 responses
        if (!operation.Responses.ContainsKey("401"))
        {
            operation.Responses.Add("401", new OpenApiResponse
            {
                Description = "Unauthorized - Authentication token is missing or invalid"
            });
        }

        if (!operation.Responses.ContainsKey("403"))
        {
            operation.Responses.Add("403", new OpenApiResponse
            {
                Description = "Forbidden - User doesn't have permission to access this resource"
            });
        }

        // Add security requirement for Bearer token
        var bearerScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                [bearerScheme] = new List<string>()
            }
        };
    }
}