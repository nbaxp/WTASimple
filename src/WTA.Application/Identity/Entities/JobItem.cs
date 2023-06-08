using System.ComponentModel.DataAnnotations;
using WTA.Shared.Attributes;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities;

[Display(Name = "工作任务")]
public class JobItem : BaseEntity
{
    [Label]
    public string Name { get; set; } = null!;
    public string Cron { get; set; } = null!;
    public string Service { get; set; } = null!;
}
