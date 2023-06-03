using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;
using WTA.Shared.Domain;

namespace WTA.Shared.Controllers;

public class GenericControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        var typeInfos = WebApp.Current.Assemblies!
            .SelectMany(o => o.GetTypes())
            .Where(o => !o.IsAbstract && o.IsAssignableTo(typeof(BaseEntity)))
            .Select(o => o.GetTypeInfo())
            .ToList();
        foreach (var entityTypeInfo in typeInfos)
        {
            var entityType = entityTypeInfo.AsType();
            if (!feature.Controllers.Any(o => o.Name == $"{entityType.Name}Controller"))
            {
                var typeInfo = typeof(GenericController<>).MakeGenericType(entityType).GetTypeInfo();
                feature.Controllers.Add(typeInfo);
            }
        }
    }
}