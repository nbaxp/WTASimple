using WTA.Shared.Domain;

namespace WTA.Application.Tenants.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Number { get; set; } = null!;
    public List<ConnectionString> ConnectionStrings { get; set; } = new List<ConnectionString>();
}
