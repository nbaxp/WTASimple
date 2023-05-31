using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace WTA.Infrastructure.Extensions;

public static class DisplayExtensions
{
    public static string GetDisplayName(this Type type)
    {
        var scope = WebApp.Current.Services?.CreateScope();
        var localizer = scope?.ServiceProvider.GetService<IStringLocalizer>();
        var key = type.GetCustomAttribute<DisplayAttribute>()?.Name ?? type.Name;
        return localizer?.GetString(key, type.FullName!) ?? key;
    }

    public static string GetDisplayName(this MemberInfo memberInfo)
    {
        var scope = WebApp.Current.Services?.CreateScope();
        var localizer = scope?.ServiceProvider.GetService<IStringLocalizer>();
        var key = memberInfo.GetCustomAttribute<SwaggerOperationAttribute>()?.Summary ?? memberInfo.GetCustomAttribute<DisplayAttribute>()?.Name ?? memberInfo.Name;
        return localizer?.GetString(key, $"{memberInfo.ReflectedType!.Name}.{key}") ?? key;
    }

    public static string GetDisplayName(this PropertyInfo propertyInfo)
    {
        var scope = WebApp.Current.Services?.CreateScope();
        var localizer = scope?.ServiceProvider.GetService<IStringLocalizer>();
        var key = propertyInfo.GetCustomAttribute<DisplayAttribute>()?.Name ?? propertyInfo.Name;
        return localizer?.GetString(key, $"{propertyInfo.ReflectedType!.Name}.{key}") ?? key;
    }
}