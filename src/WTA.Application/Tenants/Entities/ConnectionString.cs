using System.ComponentModel.DataAnnotations;
using WTA.Shared.Attributes;
using WTA.Shared.Domain;

namespace WTA.Application.Tenants.Entities;

[Display(Name = "连接字符串")]
public class ConnectionString : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;

    public Guid? ParentId { get; set; }

    public Tenant? Tenant { get; set; }
}
