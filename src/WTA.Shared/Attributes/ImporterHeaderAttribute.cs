namespace WTA.Shared.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ImporterHeaderAttribute : Attribute
{
    public string Name { get; set; } = null!;
    public bool IsIgnore { get; set; }
    public bool IsAllowRepeat { get; set; }
    public string ShowInputMessage { get; set; } = null!;
    public string Format { get; set; } = null!;
}