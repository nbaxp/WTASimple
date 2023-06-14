namespace WTA.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class OptionsAttribute : Attribute
{
    public OptionsAttribute(string? section = null)
    {
        this.Section = section;
    }

    public string? Section { get; }
}
