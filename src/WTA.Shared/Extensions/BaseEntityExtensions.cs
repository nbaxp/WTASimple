using WTA.Shared.Domain;

namespace WTA.Shared.Extensions;

public static class BaseEntityExtensions
{
    public static TEntity SetIdBy<TEntity>(this TEntity entity, Func<TEntity, object> expression)
        where TEntity : BaseEntity
    {
        entity.Id = $"{entity.TenantId},{expression.Invoke(entity)}".ToGuid();
        return entity;
    }

    public static T UpdatePath<T>(this BaseTreeEntity<T> entity, BaseTreeEntity<T>? parent = null) where T : BaseEntity
    {
        entity.Id = $"{entity.TenantId},{entity.Number}".ToGuid();
        if (parent != null)
        {
            if (parent.InternalPath == null)
            {
                entity.InternalPath = $"{parent.Id}";
            }
            else
            {
                entity.InternalPath = $"{parent.InternalPath},{parent.Id}";
            }
        }
        if (entity.Children.Any())
        {
            entity.Children.ForEach(o => (o as BaseTreeEntity<T>)!.UpdatePath(entity));
        }
        return (entity as T)!;
    }
}
