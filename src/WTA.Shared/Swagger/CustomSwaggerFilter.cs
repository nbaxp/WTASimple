using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WTA.Shared.Extensions;

namespace WTA.Shared.Swagger;

public class CustomSwaggerFilter : ISchemaFilter, IOperationFilter
{
    private readonly RequestLocalizationOptions _options;

    public CustomSwaggerFilter(IOptions<RequestLocalizationOptions> options)
    {
        this._options = options.Value;
    }
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters.ForEach(o =>
        {
            if (o.In == ParameterLocation.Path && o.Name == "culture")
            {
                o.AllowEmptyValue = true;
                o.Required = false;
                o.Schema.Nullable = true;
                o.Schema.Default = new OpenApiString("zh");
            }
        });
    }
}
