using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using WTA.Shared.Attributes;
using WTA.Shared.Extensions;

namespace WTA.Shared.Controllers;

public class ControllerModelConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (controller.ControllerType.FullName!.StartsWith(WebApp.Current.Prefix))
        {
            if (controller.ApiExplorer.GroupName == null || controller.ControllerName == controller.ApiExplorer.GroupName)
            {
                var types = controller.ControllerType.GetBaseClasses().Concat(new Type[] { controller.ControllerType });
                var genericControllerType = types.FirstOrDefault(o => o.IsGenericType && o.GetGenericTypeDefinition() == typeof(GenericController<,,,,,>));
                if (genericControllerType != null)
                {
                    var entityType = genericControllerType.GetGenericArguments().FirstOrDefault();
                    if (entityType != null)
                    {
                        var groupAttribute = entityType.GetCustomAttributes().FirstOrDefault(o => o.GetType().IsAssignableTo(typeof(GroupAttribute)));
                        var moduleType = groupAttribute?.GetType().GetCustomAttributes()
                                   .Where(o => o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition() == typeof(ModuleAttribute<>))
                                   .Select(o => o as ITypeAttribute).Select(o => o?.Type).FirstOrDefault();
                        if (moduleType != null)
                        {
                            controller.ApiExplorer.GroupName = moduleType?.Name;
                        }
                    }
                }
            }
        }
    }
}
