using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace WTA.Infrastructure.Authentication;

public class CustomJwtBearerPostConfigureOptions : JwtBearerPostConfigureOptions, IPostConfigureOptions<JwtBearerOptions>
{
    private readonly CustomJwtSecurityTokenHandler _customJwtSecurityTokenHandler;

    public CustomJwtBearerPostConfigureOptions(CustomJwtSecurityTokenHandler customJwtSecurityTokenHandler)
    {
        _customJwtSecurityTokenHandler = customJwtSecurityTokenHandler;
    }

    public new void PostConfigure(string? name, JwtBearerOptions options)
    {
        options.SecurityTokenValidators.Clear();
        options.SecurityTokenValidators.Add(_customJwtSecurityTokenHandler);
        base.PostConfigure(name, options);
    }
}