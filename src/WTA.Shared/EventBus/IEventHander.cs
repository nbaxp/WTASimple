namespace WTA.Shared.EventBus;

public interface IEventHander<T>
{
    Task Handle(T data);
}
