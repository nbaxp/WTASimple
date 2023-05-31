namespace WTA.Infrastructure.EventBus;

public interface IEventPublisher
{
    Task Publish<T>(T data);
}