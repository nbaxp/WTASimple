namespace WTA.Shared.Tenants;

public interface ITenantService
{
    string? TenantId { get; set; }

    string? GetConnectionString(string connectionStringName);
}
