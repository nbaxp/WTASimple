using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Distributed;
using WTA.Shared.Captcha;

namespace WTA.Application.Captcha;

[Route("api/[controller]")]
public class CaptchaController : Controller
{
    private readonly IDistributedCache _cache;
    private readonly ICaptchaService _captchaService;

    public CaptchaController(IDistributedCache cache, ICaptchaService captchaService)
    {
        this._cache = cache;
        this._captchaService = captchaService;
    }

    [HttpGet]
    [AllowAnonymous]
    [OutputCache(NoStore = true)]
    public IActionResult Index()
    {
        var code = string.Empty;
        var builder = new StringBuilder();
        builder.Append(code);
        for (var i = 0; i < 4; i++)
        {
            var random = new byte[1];
            using var generator = RandomNumberGenerator.Create();
            generator.GetBytes(random);
            builder.Append(new Random(Convert.ToInt32(random[0])).Next(0, 9));
        }
        code = builder.ToString();
        var key = Guid.NewGuid().ToString();
        this._cache.SetString(key, code, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(5) });
        return Json(new
        {
            Captcha = this._captchaService.Create(code),
            CaptchaKey = key
        });
    }
}
