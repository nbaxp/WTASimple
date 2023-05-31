using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WTA.Infrastructure.Swagger;

public class CustomSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiDescriptionGroupCollectionProvider provider;

    public CustomSwaggerGenOptions(IApiDescriptionGroupCollectionProvider provider)
    {
        this.provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiDescriptionGroups.Items)
        {
            if (description.GroupName is not null)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo { Title = description.GroupName });
            }
            else
            {
                options.SwaggerDoc("Default", new OpenApiInfo { Title = "Default" });
            }
        }
    }
}
