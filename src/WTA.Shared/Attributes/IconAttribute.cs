namespace WTA.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class IconAttribute : Attribute
{
    public IconAttribute(string icon)
    {
        this.Icon = icon;
    }

    public static string File { get; } = "file";
    public static string Folder { get; } = "folder";
    public string? Icon { get; } = File;
}
