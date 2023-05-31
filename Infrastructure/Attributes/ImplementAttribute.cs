using WTA.Infrastructure.DependencyInjection;

namespace WTA.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ImplementAttribute<T> : Attribute, IImplementAttribute
{
    public ImplementAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient, PlatformType platformType = PlatformType.All)
    {
        this.ServiceType = typeof(T);
        this.Lifetime = lifetime;
        this.PlatformType = platformType;
    }

    public ServiceLifetime Lifetime { get; set; }
    public PlatformType PlatformType { get; set; }
    public Type ServiceType { get; }
}

public interface IImplementAttribute
{
    ServiceLifetime Lifetime { get; }
    PlatformType PlatformType { get; }
    Type ServiceType { get; }
}