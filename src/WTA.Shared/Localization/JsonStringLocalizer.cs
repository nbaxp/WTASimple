using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text.Json;

namespace WTA.Shared.Localization;

public class JsonStringLocalizer : IStringLocalizer
{
    private readonly IMemoryCache _cache;

    public JsonStringLocalizer(IMemoryCache cache)
    {
        this._cache = cache;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = GetString(name);
            return new LocalizedString(name, value ?? name, value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var actualValue = this[name];
            return !actualValue.ResourceNotFound
                ? new LocalizedString(name, string.Format(CultureInfo.InvariantCulture, actualValue.Value, arguments), false)
                : actualValue;
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return GetAll().Select(o => new LocalizedString(o.Key, o.Value));
    }

    private Dictionary<string, string> GetAll()
    {
        var key = $"{nameof(JsonStringLocalizer)}.{Thread.CurrentThread.CurrentCulture.Name}";
        var result = this._cache.Get<Dictionary<string, string>>(key);
        if (result == null)
        {
            result = new Dictionary<string, string>();
            WebApp.Current.Assemblies?
           //.Concat(new Assembly[] { typeof(Resource).Assembly })
           .OrderBy(o => o.FullName!.Length)
           .ToList()
           .ForEach(assembly =>
           {
               var filePath = $"{assembly.GetName().Name}.Resources.{Thread.CurrentThread.CurrentCulture.Name}.json";
               using var stream = assembly.GetManifestResourceStream(filePath);
               if (stream is not null)
               {
                   using var jdoc = JsonDocument.Parse(stream);
                   var keyValues = jdoc.Deserialize<Dictionary<string, string>>();
                   foreach (var item in keyValues!)
                   {
                       result[item.Key] = item.Value;
                   }
               }
           });
        }
        return result;
    }

    private string GetString(string key)
    {
        var _dictionary = GetAll();
        if (_dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        if (key.Contains('.') && _dictionary.TryGetValue(key[(key.IndexOf('.') + 1)..], out var value2))
        {
            return value2;
        }
        return key;
    }
}
