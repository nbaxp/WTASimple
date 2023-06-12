namespace WTA.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class OrderAttribute : Attribute
{
    public OrderAttribute(int order)
    {
        this.Order = order;
    }

    public static int Default { get; }
    public int? Order { get; } = Default;
}
