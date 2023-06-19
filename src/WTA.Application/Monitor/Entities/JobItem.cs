using WTA.Shared.Domain;

namespace WTA.Application.Monitor.Entities;

public class JobItem : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Cron { get; set; } = null!;
    public string Service { get; set; } = null!;
}
