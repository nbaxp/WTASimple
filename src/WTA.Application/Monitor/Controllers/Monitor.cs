using WTA.Shared.Application;
using WTA.Shared.Attributes;

namespace WTA.Application.Monitor.Controllers;

[Order(3)]
[SystemMonitor]
[Component("monitor")]
public class Monitor : IResource
{
}
