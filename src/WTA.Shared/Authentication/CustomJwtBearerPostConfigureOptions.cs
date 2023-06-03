using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace WTA.Shared.Authentication;

public class CustomJwtBearerPostConfigureOptions : JwtBearerPostConfigureOptions, IPostConfigureOptions<JwtBearerOptions>
{
    private readonly CustomJwtSecurityTokenHandler _customJwtSecurityTokenHandler;

    public CustomJwtBearerPostConfigureOptions(CustomJwtSecurityTokenHandler customJwtSecurityTokenHandler)
    {
        this._customJwtSecurityTokenHandler = customJwtSecurityTokenHandler;
    }

    public new void PostConfigure(string? name, JwtBearerOptions options)
    {
        options.SecurityTokenValidators.Clear();
        options.SecurityTokenValidators.Add(this._customJwtSecurityTokenHandler);
        base.PostConfigure(name, options);
    }
}
