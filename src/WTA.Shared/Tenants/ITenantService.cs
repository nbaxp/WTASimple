namespace WTA.Shared.Tenants;

public interface ITenantService
{
    string? GetTenantId();

    string? GetConnectionString(string connectionStringName);
}
