namespace WTA.Shared.EventBus;

public class EntityDeletedEvent<T> : BaseEvent<T>
{
    public EntityDeletedEvent(T entity) : base(entity, nameof(EntityDeletedEvent<T>))
    {
    }
}
