using WTA.Application.Tenants.Entities;
using WTA.Shared.Data;
using WTA.Shared.Extensions;

namespace WTA.Application.Identity.Data;

public class TenantDbSeed : IDbSeed<TenantDbContext>
{
    public void Seed(TenantDbContext context)
    {
        context.Set<Tenant>().Add(new Tenant
        {
            Name = "默认租户",
            Number = "default",
            DataBaseCreated = false,
            ConnectionStrings = new List<ConnectionString>
            {
                new ConnectionString(){ Name="Tenant",Value="Data Source=data2.db" },
                new ConnectionString(){ Name="Identity",Value="Data Source=data2.db" }
            }
        }.SetIdBy(o => o.Number));
    }
}
