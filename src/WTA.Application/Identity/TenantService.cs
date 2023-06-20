using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WTA.Application.Identity.Entities.Tenants;
using WTA.Shared.Attributes;
using WTA.Shared.Data;
using WTA.Shared.Tenants;

namespace WTA.Application.Identity;

[Implement<ITenantService>]
public class TenantService : ITenantService
{
    private readonly string? _tenant;
    private readonly IServiceProvider _serviceProvider;

    public TenantService(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        this._tenant = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(o => o.Type == "TenantId")?.Value;
        this._serviceProvider = serviceProvider;
    }

    public string? GetTenantId()
    {
        return this._tenant;
    }

    public string? GetConnectionString(string connectionStringName)
    {
        using var scope = this._serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<Tenant>>();
        repository.DisableTenantFilter();
        return repository
            .AsNoTracking()
            .Where(o => o.Number == this._tenant)
            .SelectMany(o => o.ConnectionStrings)
            .FirstOrDefault(o => o.Name == connectionStringName)
            ?.Value;
    }
}
