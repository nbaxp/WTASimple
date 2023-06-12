using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
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
                var modelType = entityType;
                var listType = entityType;
                var searchType = entityType;
                var importType = entityType;
                var exportType = entityType;
                var controllerType = typeof(GenericController<,,,,,>).MakeGenericType(entityType, modelType, listType, searchType, importType, exportType);
                feature.Controllers.Add(controllerType.GetTypeInfo());
            }
        }
    }
}
