using WTA.Infrastructure.Application;
using WTA.Infrastructure.Extensions;

namespace WTA.Infrastructure.Domain;

public abstract class BaseEntity : IResource
{
    public BaseEntity()
    {
        this.Init();
    }

    public Guid Id { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsReadonly { get; set; }
    public int? Order { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public string? TenantId { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
}