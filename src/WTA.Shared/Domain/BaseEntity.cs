using System.ComponentModel.DataAnnotations;
using WTA.Shared.Application;
using WTA.Shared.Extensions;

namespace WTA.Shared.Domain;

public abstract class BaseEntity : IResource, ISoftDeleted
{
    public BaseEntity()
    {
        this.Init();
    }

    [ScaffoldColumn(false)]
    public Guid Id { get; set; }

    //[ScaffoldColumn(false)]
    public bool IsDeleted { get; set; }

    public bool? IsDisabled { get; set; }
    public bool? IsReadonly { get; set; }
    public int Order { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    [ScaffoldColumn(false)]
    public string ConcurrencyStamp { get; set; } = null!;

    [ScaffoldColumn(false)]
    public string? TenantId { get; set; }

    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
}
