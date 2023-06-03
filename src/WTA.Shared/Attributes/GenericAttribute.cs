namespace WTA.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class GenericAttribute<T> : Attribute, ITypeAttribute
{
    public Type Type => typeof(T);
}

public interface ITypeAttribute
{
    Type Type { get; }
}