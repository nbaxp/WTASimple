using WTA.Shared.Attributes;

namespace WTA.Shared.Tenants;

[Options]
public class TenantsOptions
{
    public bool IsEnabled { get; set; }
    public bool DatabasePerTenant { get; set; }
}
