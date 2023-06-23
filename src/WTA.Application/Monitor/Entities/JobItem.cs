using WTA.Application.Monitor.Controllers;
using WTA.Shared.Attributes;
using WTA.Shared.Domain;

namespace WTA.Application.Monitor.Entities;

[Order(2)]
[SystemMonitor]
public class JobItem : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Cron { get; set; } = null!;
    public string Service { get; set; } = null!;
}
