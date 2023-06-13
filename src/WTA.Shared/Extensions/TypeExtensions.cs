using System.Reflection;

namespace WTA.Shared.Extensions;

public static class TypeExtensions
{
    public static bool HasAttribute<T>(this Type type, bool inherit = true) where T : Attribute
    {
        return type.GetCustomAttributes<T>(inherit).Any();
    }

    public static Type[] GetBaseClasses(this Type type)
    {
        List<Type> classes = new();
        var current = type;
        while (current.BaseType != null && current.BaseType.IsClass && current.BaseType != typeof(object))
        {
            classes.Add(current.BaseType);
            current = current.BaseType;
        }
        return classes.ToArray();
    }

    public static bool IsNullableType(this Type type)
    {
        return (((type != null) && type.IsGenericType) && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
    }

    public static Type GetUnderlyingType(this Type type)
    {
        return type.IsNullableType() ? Nullable.GetUnderlyingType(type)! : type;
    }
}
