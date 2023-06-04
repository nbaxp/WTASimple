using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WTA.Shared.Controllers;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class GenericControllerNameConventionAttribute : Attribute, IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (controller.ControllerType.IsGenericType)
        {
            var controllerType = controller.ControllerType.IsGenericType ? controller.ControllerType : controller.ControllerType.BaseType!;
            if (controllerType.GetGenericTypeDefinition() == typeof(GenericController<>))
            {
                var entityType = controllerType.GenericTypeArguments[0];
                if (controller.ControllerName != entityType.Name)
                {
                    controller.ControllerName = entityType.Name!;
                }
                var moduleName = entityType.Assembly.GetName().Name;
                if (string.IsNullOrEmpty(controller.ApiExplorer.GroupName) && !string.IsNullOrEmpty(moduleName))
                {
                    controller.ApiExplorer.GroupName = moduleName;
                }
            }
        }
    }
}
