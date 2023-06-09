namespace WTA.Shared.Domain;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class PropertyAttribute : Attribute
{
    public PropertyAttribute(PropertyType propertyType)
    {
        this.PropertyType = propertyType;
    }

    public PropertyType PropertyType { get; }
}

[Flags]
public enum PropertyType
{
    DisableList = 1,
    DisableCreate = 2,
    DisableUpdate = 4,
}
