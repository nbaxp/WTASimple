namespace WTA.Shared.Mappers;

public interface IObjectMapper
{
    void FromObject<T>(T to, object from, params string[] properties);

    T ToObject<T>(object from);
}
