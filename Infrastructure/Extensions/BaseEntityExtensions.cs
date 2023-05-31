using Microsoft.AspNetCore.WebUtilities;
using WTA.Infrastructure.Domain;
using WTA.Infrastructure.GuidGenerators;

namespace WTA.Infrastructure.Extensions;

public static class BaseEntityExtensions
{
    public static void Init(this BaseEntity entity)
    {
        using var scope = WebApp.Current.Services.CreateScope();
        entity.Id = scope.ServiceProvider.GetRequiredService<IGuidGenerator>().Create();
        entity.ConcurrencyStamp ??= Guid.NewGuid().ToString("N");
        //entity.TenantId = scope.ServiceProvider.GetRequiredService<ICurrentUser>().TenantId;
        //entity.CreationTime = DateTime.Now;
        //entity.CreatorId = scope.ServiceProvider.GetRequiredService<ICurrentUser>().Id;
    }

    public static TEntity SetIdBy<TEntity>(this TEntity entity, Func<TEntity, object> expression)
        where TEntity : BaseEntity
    {
        entity.Id = $"{entity.TenantId},{expression.Invoke(entity)}".ToGuid();
        return entity;
    }

    public static T UpdatePath<T>(this BaseTreeEntity<T> entity, BaseTreeEntity<T>? parent = null) where T : class
    {
        entity.Id = $"{entity.TenantId},{parent?.Number},{entity.Number}".ToGuid();
        entity.InternalPath = $"/{WebEncoders.Base64UrlEncode(entity.Id.ToByteArray())}";
        if (parent != null)
        {
            entity.InternalPath = $"{parent.InternalPath}{entity.InternalPath}";
        }
        if (entity.Children.Any())
        {
            entity.Children.ForEach(o => (o as BaseTreeEntity<T>)!.UpdatePath(entity));
        }
        return (entity as T)!;
    }
}