namespace WTA.Infrastructure.EventBus;

public interface IEventHander<T>
{
    Task Handle(T data);
}