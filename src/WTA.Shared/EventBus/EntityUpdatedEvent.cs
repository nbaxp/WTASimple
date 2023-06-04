namespace WTA.Shared.EventBus;

public class EntityUpdatedEvent<T> : BaseEvent<T>
{
    public EntityUpdatedEvent(T entity) : base(entity, nameof(EntityUpdatedEvent<T>))
    {
    }
}
