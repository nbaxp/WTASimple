namespace WTA.Infrastructure.EventBus;

public class BaseEvent<T>
{
    public BaseEvent(T data, string type)
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
        Type = type;
        Data = data;
    }

    public Guid Id { get; }
    public DateTime CreationDate { get; }
    public string Type { get; }
    public T Data { get; }
}