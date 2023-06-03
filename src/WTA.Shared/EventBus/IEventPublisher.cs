namespace WTA.Shared.EventBus;

public interface IEventPublisher
{
    Task Publish<T>(T data);
}