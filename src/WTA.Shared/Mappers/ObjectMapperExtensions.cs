using Microsoft.Extensions.DependencyInjection;
using WTA.Shared.Domain;

namespace WTA.Shared.Mappers;

public static class ObjectMapperExtensions
{
    public static T ToObject<T>(this object from)
    {
        using var scope = WebApp.Current.Services!.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IObjectMapper>().ToObject<T>(from);
    }

    public static T FromObject<T>(this T target, object from)
    {
        using var scope = WebApp.Current.Services!.CreateScope();
        scope.ServiceProvider.GetRequiredService<IObjectMapper>().FromObject(target, from);
        return target;
    }

    /// <summary>
    /// update entity from model,skip IEntity<Guid>, IAuditedObject, IHasConcurrencyStamp, IMultiTenant
    /// </summary>
    public static T FromModel<T>(this T target, object from)
    {
        var properties = new List<Type>() { typeof(IBaseEntity), typeof(IBaseEntity), typeof(IBaseEntity) }.SelectMany(o => o.GetProperties()).Select(o => o.Name).ToArray(); ;
        using var scope = WebApp.Current.Services!.CreateScope();
        scope.ServiceProvider.GetRequiredService<IObjectMapper>().FromObject(target, from, properties);
        return target;
    }
}
