using System.ComponentModel.DataAnnotations;
using WTA.Shared.Attributes;
using WTA.Shared.Domain;

namespace WTA.Application.Tenants.Entities;

[Display(Name = "租户")]
public class Tenant : BaseEntity
{
    [Label]
    public string Name { get; set; } = null!;
    public string Number { get; set; } = null!;
    public bool DataBaseCreated { get; set; }
    public List<ConnectionString> ConnectionStrings { get; set; } = new List<ConnectionString>();
}
