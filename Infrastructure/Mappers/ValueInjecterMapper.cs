using Omu.ValueInjecter;

namespace WTA.Infrastructure.Mappers;

public class ValueInjecterMapper : IObjectMapper
{
    public void FromObject<T>(T to, object from, params string[] properties)
    {
        if (properties.Length > 0)
        {
            to.InjectFrom(new FromInjection(properties), from);
        }
        else
        {
            to.InjectFrom<FromInjection>(from);
        }
    }

    public T ToObject<T>(object from)
    {
        if (Mapper.Instance.Maps.Any(o => o.Key.Item1 == from.GetType() && o.Key.Item2 == typeof(T)))
        {
            return Mapper.Map<T>(from);
        }
        else
        {
            var result = Activator.CreateInstance<T>();
            result.InjectFrom<ToInjection>(from);
            return result;
        }
    }
}