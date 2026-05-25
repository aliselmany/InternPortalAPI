using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using InternPortal.Infrastructure.Persistence; 
using Microsoft.Extensions.DependencyInjection;

namespace InternPortal.Api.Filters;

public class RoleDropdownFilter : IOperationFilter
{
    private readonly IServiceProvider _serviceProvider;

    public RoleDropdownFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var roleParam = operation.Parameters?.FirstOrDefault(p =>
            p.Name.Contains("role", StringComparison.OrdinalIgnoreCase));

        if (roleParam != null)
        {
            UpdateSchemaWithRoles(roleParam.Schema);
        }
       
        if (operation.RequestBody?.Content.ContainsKey("application/json") == true)
        {
            var schema = operation.RequestBody.Content["application/json"].Schema;

            var roleProp = schema.Properties?.FirstOrDefault(p =>
                p.Key.Contains("role", StringComparison.OrdinalIgnoreCase)).Value;

            if (roleProp != null)
            {
                UpdateSchemaWithRoles(roleProp);
            }
        }
    }

    private void UpdateSchemaWithRoles(OpenApiSchema schema)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                var roles = dbContext.Roles.Select(r => r.Name).ToList();

                if (roles.Any())
                {
                    schema.Enum = roles.Select(r => (IOpenApiAny)new OpenApiString(r)).ToList();
                    schema.Type = "string";
                }
            }
            catch
            {
 
            }
        }
    }
}