using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WTA.Shared.Authentication;

public class CustomJwtSecurityTokenHandler : JwtSecurityTokenHandler
{
    private readonly IServiceProvider _serviceProvider;

    public CustomJwtSecurityTokenHandler(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }

    public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
    {
        return new CustomClaimsPrincipal(this._serviceProvider, base.ValidateToken(token, validationParameters, out validatedToken));
    }
}