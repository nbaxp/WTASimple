namespace WTA.Shared.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class HtmlClassAttribute : Attribute
{
    public HtmlClassAttribute(string @class)
    {
        this.Class = @class;
    }

    public static string Default { get; } = "el-button--primary";
    public string? Class { get; } = Default;
}
