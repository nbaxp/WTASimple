using System.Reflection;
using WTA.Shared.Attributes;

namespace WTA.Shared.Mappers;

public class FromInjection : ToInjection
{
    private readonly string[] properties;

    public FromInjection()
    {
        this.properties = Array.Empty<string>();
    }

    public FromInjection(string[] properties)
    {
        this.properties = properties;
    }

    protected override void SetValue(object source, object target, PropertyInfo sp, PropertyInfo tp)
    {
        if (this.properties.Contains(tp.Name))
        {
            return;
        }
        if (tp.GetCustomAttribute<IgnoreUpdateAttribute>() != null)
        {
            return;
        }
        base.SetValue(source, target, sp, tp);
    }
}