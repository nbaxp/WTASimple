namespace WTA.Shared.Extensions;

public static class ObjectExtensions
{
    public static TProperty? GetPropertyValue<TObject, TProperty>(this TObject @object, string property) where TObject : class
    {
        return (TProperty?)(@object.GetType().GetProperty(property)?.GetValue(@object));
    }
}
