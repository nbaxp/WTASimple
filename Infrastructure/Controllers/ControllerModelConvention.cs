using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace WTA.Infrastructure.Controllers;

public class ControllerModelConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (controller.ControllerType.FullName!.StartsWith(WebApp.Current.Prefix))
        {
            if (controller.ApiExplorer.GroupName == null || controller.ControllerName == controller.ApiExplorer.GroupName)
            {
                controller.ApiExplorer.GroupName = controller.ControllerType.Assembly.GetName().Name;
            }
        }
    }
}
