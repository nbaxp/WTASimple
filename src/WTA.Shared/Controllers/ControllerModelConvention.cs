using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
                var genericControllerType = types.FirstOrDefault(o => o.IsGenericType && o.GetGenericTypeDefinition() == typeof(GenericController<>));
                if (genericControllerType != null)
                {
                    var entityType = genericControllerType.GetGenericArguments().FirstOrDefault();
                    if (entityType != null)
                    {
                        controller.ApiExplorer.GroupName = WebApp.Current.ModuleTypes.Where(o => o.Value.Values.Any(o => o.Contains(entityType))).Select(o => o.Key).FirstOrDefault()?.Name ?? controller.ControllerType.Assembly.GetName().Name;
                    }
                }
            }
        }
    }
}
