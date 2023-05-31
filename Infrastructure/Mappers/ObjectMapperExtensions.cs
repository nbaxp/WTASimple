using WTA.Infrastructure.Domain;

namespace WTA.Infrastructure.Mappers;

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
        using var scope = WebApp.Current.Services!.CreateScope();
        scope.ServiceProvider.GetRequiredService<IObjectMapper>().FromObject(target, from, typeof(BaseEntity).GetProperties().Select(o => o.Name).ToArray());
        return target;
    }
}