namespace WTA.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class OrderAttribute : Attribute
{
    public OrderAttribute(int order = 0)
    {
        this.Order = order;
    }

    public int Order { get; }
}