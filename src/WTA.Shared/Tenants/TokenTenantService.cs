using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WTA.Shared.Attributes;

namespace WTA.Shared.Tenants;

[Implement<ITenantService>(ServiceLifetime.Scoped)]
public class TokenTenantService : ITenantService
{
    private readonly string? _tenant;

    public TokenTenantService(IHttpContextAccessor httpContextAccessor)
    {
        this._tenant = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(o => o.Type == "TenantId")?.Value; ;
    }

    public string? GetTenantId()
    {
        return this._tenant;
    }
}
