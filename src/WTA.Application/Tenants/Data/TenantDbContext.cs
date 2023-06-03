using Microsoft.EntityFrameworkCore;
using WTA.Shared.Attributes;
using WTA.Shared.Data;

namespace WTA.Application.Tenants.Entities;

[Module<TenantModule>]
public class TenantDbContext : BaseDbContext<TenantDbContext>
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options)
    {
    }
}
