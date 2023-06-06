using System.ComponentModel.DataAnnotations;
using WTA.Shared.Domain;

namespace WTA.Application.Tenants.Entities;

[Display(Name = "租户")]
public class Tenant : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Number { get; set; } = null!;
    public List<ConnectionString> ConnectionStrings { get; set; } = new List<ConnectionString>();
}
