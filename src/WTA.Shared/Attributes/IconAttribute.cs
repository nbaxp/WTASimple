namespace WTA.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class IconAttribute : Attribute
{
    public IconAttribute(string icon = "default")
    {
        this.Icon = icon;
    }

    public string Icon { get; }
}
