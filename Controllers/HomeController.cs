using Microsoft.AspNetCore.Mvc;

namespace WTA.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return File("~/index.html", "text/html");
    }
}