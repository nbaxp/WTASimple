using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using WTA.Shared.Application;
using WTA.Shared.GuidGenerators;

namespace WTA.Shared.Domain;

public abstract class BaseEntity : IResource, IBaseEntity, ISoftDeleteEntity, IAuditEntity
{
    public BaseEntity()
    {
        this.IsDeleted = false;
        this.IsDisabled = false;
        this.IsReadonly = false;
        using var scope = WebApp.Current.Services.CreateScope();
        this.Id = scope.ServiceProvider.GetRequiredService<IGuidGenerator>().Create();
    }

    [ReadOnly(true)]
    public bool IsDeleted { get; set; }

    [Required]
    public bool? IsDisabled { get; set; }

    [Required]
    public bool? IsReadonly { get; set; }

    public int Order { get; set; }

    [Required]
    [ReadOnly(true)]
    public DateTime? CreatedOn { get; set; }

    [ReadOnly(true)]
    public string? CreatedBy { get; set; }

    [ReadOnly(true)]
    public DateTime? UpdatedOn { get; set; }

    [ReadOnly(true)]
    public string? UpdatedBy { get; set; }

    [ReadOnly(true)]
    public DateTime? DeletedOn { get; set; }

    [ReadOnly(true)]
    public string? DeletedBy { get; set; }

    [ScaffoldColumn(false)]
    public Guid Id { get; set; }

    [ScaffoldColumn(false)]
    public string ConcurrencyStamp { get; set; } = null!;

    [ScaffoldColumn(false)]
    public string? TenantId { get; set; }

    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
}
