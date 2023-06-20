using System.ComponentModel.DataAnnotations;
using WTA.Shared.Attributes;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities.Tenants;

[Tenants]
public class ConnectionString : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;

    [Navigation]
    public Guid? ParentId { get; set; }

    public Tenant? Parent { get; set; }
}
