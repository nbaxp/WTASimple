namespace WTA.Shared.EventBus;

public class BaseEvent<T>
{
    public BaseEvent(T data, string type)
    {
        this.Id = Guid.NewGuid();
        this.CreationDate = DateTime.UtcNow;
        this.Type = type;
        this.Data = data;
    }

    public Guid Id { get; }
    public DateTime CreationDate { get; }
    public string Type { get; }
    public T Data { get; }
}
