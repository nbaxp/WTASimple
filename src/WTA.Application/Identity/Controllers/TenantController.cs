using Microsoft.Extensions.Logging;
using WTA.Application.Identity.Entities.Tenants;
using WTA.Shared.Controllers;
using WTA.Shared.Data;

namespace WTA.Application.Identity.Controllers;

public class TenantController : GenericController<Tenant, Tenant, Tenant, Tenant, Tenant, Tenant>
{
    public TenantController(ILogger<Tenant> logger, IRepository<Tenant> repository) : base(logger, repository)
    {
        this.Repository.DisableTenantFilter();
    }
}
