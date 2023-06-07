using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using WTA.Shared.Attributes;
using WTA.Shared.Extensions;

namespace WTA.Shared.Controllers;

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
            routeTemplate += "api/{culture=zh}/[controller]/[action]";
            controller.Selectors.Add(new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(routeTemplate)),
            });
            controller.Actions.ForEach(action =>
            {
                if (!action.Attributes.Any(o => o.GetType().IsAssignableTo(typeof(HttpMethodAttribute))))
                {
#pragma warning disable SYSLIB1045 // 转换为“GeneratedRegexAttribute”。
                    var match = Regex.Match(action.ActionName, "^(Get|Post|Put|Delete|Patch|Head|Options)");
#pragma warning restore SYSLIB1045 // 转换为“GeneratedRegexAttribute”。
                    if (match.Success)
                    {
                        var method = match.Groups[1].Value;
                        var actionName = action.ActionName.TrimStart(method);
                        (action.Attributes as List<object>)?.Add(new HttpMethodDefaultAttribute(new List<string> { method }));
                    }
                }
            });
        }
    }
}
