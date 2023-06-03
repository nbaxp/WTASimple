using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace WTA.Shared.Extensions;

public static class JsonExtensions
{
    public static string ToJson(this object instance)
    {
        var scope = WebApp.Current.Services?.CreateScope();
        var options = scope?.ServiceProvider.GetService<JsonSerializerOptions>();
        return JsonSerializer.Serialize(instance, options);
    }

    public static T? FromJson<T>(this string json)
    {
        var scope = WebApp.Current.Services?.CreateScope();
        var options = scope?.ServiceProvider.GetService<JsonSerializerOptions>();
        return JsonSerializer.Deserialize<T>(json, options);
    }
}
