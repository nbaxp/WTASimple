using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using WTA.Shared.Controllers;

namespace WTA.Application.Localization;

[Route("api/[controller]")]
public class LocalizationController : BaseController
{
    private readonly IStringLocalizer _localizer;
    private readonly RequestLocalizationOptions _options;

    public LocalizationController(IOptions<RequestLocalizationOptions> options, IStringLocalizer localizer)
    {
        this._options = options.Value;
        this._localizer = localizer;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index(string? culture)
    {
        if (culture != null)
        {
            Thread.CurrentThread.CurrentCulture = this._options.SupportedCultures!.First(o => o.Name == culture);
        }
        var result = new
        {
            Options = this._options.SupportedUICultures?
                .Select(o => new { Value = o.Name, Label = o.NativeName })
                .ToList(),
            Locale = Thread.CurrentThread.CurrentCulture.Name,
            Messages = new Dictionary<string, object>(),
        };
        foreach (var item in this._options.SupportedUICultures!)
        {
            Thread.CurrentThread.CurrentCulture = item;
            result.Messages.Add(item.Name, this._localizer.GetAllStrings().ToDictionary(o => o.Name, o => o.Value));
        }
        return Json(result);
    }
}
