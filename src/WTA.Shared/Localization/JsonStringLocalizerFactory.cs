using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;

namespace WTA.Shared.Localization;

public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly IMemoryCache _cache;

    public JsonStringLocalizerFactory(IMemoryCache cache)
    {
        this._cache = cache;
    }
    public IStringLocalizer Create(Type resourceSource) => new JsonStringLocalizer(this._cache);

    public IStringLocalizer Create(string baseName, string location) => new JsonStringLocalizer(this._cache);
}
