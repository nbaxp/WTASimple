using Microsoft.AspNetCore.Mvc;
using WTA.Shared.Tenants;

namespace WTA.Controllers;

public class HomeController : Controller
{
    public HomeController(ITenantService tenantService)
    {
    }

    public IActionResult Index()
    {
        return File("~/index.html", "text/html");
    }
}
