namespace WTA.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ComponentAttribute : Attribute
{
    public ComponentAttribute(string? component = null)
    {
        this.Component = component;
    }

    public string? Component { get; }
}
