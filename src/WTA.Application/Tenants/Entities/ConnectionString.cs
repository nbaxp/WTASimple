using WTA.Shared.Domain;

namespace WTA.Application.Tenants.Entities;

public class ConnectionString : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public Tenant? Tenant { get; set; }
}
