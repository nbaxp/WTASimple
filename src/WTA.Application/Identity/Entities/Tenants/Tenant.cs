using System.ComponentModel.DataAnnotations;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities.Tenants;

[Display(Name = "租户")]
[Tenants]
public class Tenant : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Number { get; set; } = null!;

    [Required]
    public bool? DataBaseCreated { get; set; }

    public List<ConnectionString> ConnectionStrings { get; set; } = new List<ConnectionString>();
}
