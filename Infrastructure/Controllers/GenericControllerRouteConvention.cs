using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;
using WTA.Infrastructure.Attributes;
using WTA.Infrastructure.Extensions;

namespace WTA.Infrastructure.Controllers;

public class GenericControllerRouteConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (controller.ControllerType.IsGenericType && controller.ControllerType.GetGenericTypeDefinition() == typeof(GenericController<>))
        {
            var genericType = controller.ControllerType.GenericTypeArguments[0];
            var moduleTypeName = genericType.Assembly.GetName().Name!.TrimStart(WebApp.Current.Prefix).TrimEnd("Application").Trim('.');
            var groupTypeName = (genericType.GetCustomAttributes().FirstOrDefault(a => a.GetType().IsAssignableTo(typeof(GroupAttribute))))?
                .GetType().Name.TrimEnd("Attribute");

            var routeTemplate = $"{moduleTypeName?.ToSlugify()}/";
            if (groupTypeName != null)
            {
                routeTemplate += $"{groupTypeName?.ToSlugify()}/";
            }
            routeTemplate += "[controller]/[action]";
            controller.Selectors.Add(new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(routeTemplate)),
            });
            //controller.Actions.ForEach(action =>
            //{
            //    if (!action.Attributes.Any(o => o.GetType().IsAssignableTo(typeof(HttpMethodAttribute))))
            //    {
            //        var match = Regex.Match(action.ActionName, "^(Get|Post|Put|Delete|Patch|Head|Options)");
            //        if (match.Success)
            //        {
            //            var method = match.Groups[1].Value;
            //            var actionName = action.ActionName.TrimStart(method);
            //            (action.Attributes as List<object>)?.Add(new HttpMethodDefaultAttribute(new List<string> { method }));
            //        }
            //    }
            //});
        }
    }
}