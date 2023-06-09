namespace WTA.Shared.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class NavigationAttribute : Attribute
{
    public NavigationAttribute(string? path = null)
    {
        this.Path = path;
    }

    public string? Path { get; }
}
