using Microsoft.AspNetCore.Mvc;
using WTA.Shared.Application;
using WTA.Shared.Extensions;
using WTA.Shared.Monitor;

namespace WTA.Application.Monitor.Controllers;

[ApiExplorerSettings(GroupName = nameof(MonitorModule))]
[Route("api/{culture}/system-monitor/[controller]/[action]")]
public class MonitorController : Controller, IResourceService<Monitor>
{
    private readonly IMonitorService _monitorService;

    public MonitorController(IMonitorService monitorService)
    {
        this._monitorService = monitorService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Json(typeof(MonitorModel).GetMetadataForType());
    }

    [HttpPost]
    public IActionResult Index(MonitorModel model)
    {
        return Json(this._monitorService.GetStatus());
    }
}
