using System.Reflection;

namespace WTA.Infrastructure.Extensions;

public static class NullabilityExtensions
{
    public static bool IsOptional(this PropertyInfo info)
    {
        var nullabilityContext = new NullabilityInfoContext();
        var nullability = nullabilityContext.Create(info!);
        var isOptional = nullability != null && nullability.ReadState != System.Reflection.NullabilityState.NotNull;
        return isOptional;
    }

    public static bool IsOptional(this ParameterInfo info)
    {
        var nullabilityContext = new NullabilityInfoContext();
        var nullability = nullabilityContext.Create(info!);
        var isOptional = nullability != null && nullability.ReadState != System.Reflection.NullabilityState.NotNull;
        return isOptional;
    }
}
