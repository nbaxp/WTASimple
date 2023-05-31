namespace WTA.Infrastructure.EventBus;

public class EntityCreatedEvent<T> : BaseEvent<T>
{
    public EntityCreatedEvent(T entity) : base(entity, nameof(EntityCreatedEvent<T>))
    {
    }
}