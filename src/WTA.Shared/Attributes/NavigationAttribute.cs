namespace WTA.Shared.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class NavigationAttribute : Attribute
{
    public NavigationAttribute(string? property = null)
    {
        this.Property = property;
    }

    public string? Property { get; }
}
