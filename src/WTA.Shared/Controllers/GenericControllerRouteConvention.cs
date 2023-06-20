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
        var baseControllerType = controller.ControllerType.GetBaseClasses().Concat(new Type[] { controller.ControllerType }).FirstOrDefault(o => o.IsGenericType && o.GetGenericTypeDefinition() == typeof(GenericController<,,,,,>));
        if (baseControllerType != null)
        {
            var routeTemplate = $"api/{{culture=zh}}/";
            var genericType = baseControllerType.GenericTypeArguments[0];
            var groupAttribute = genericType.GetCustomAttributes().FirstOrDefault(o => o.GetType().IsAssignableTo(typeof(GroupAttribute)));
            var moduleAttribute = groupAttribute?.GetType().GetCustomAttributes()
                       .Where(o => o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition() == typeof(ModuleAttribute<>))
                       .Select(o => o as ITypeAttribute).Select(o => o?.Type).FirstOrDefault();
            if (moduleAttribute != null)
            {
                routeTemplate += $"{moduleAttribute.Name.TrimEnd("Module").ToSlugify()}/";
            }
            if (groupAttribute != null)
            {
                routeTemplate += $"{groupAttribute.GetType().Name.TrimEnd("Attribute").ToSlugify()}/";
            }
            routeTemplate += "[controller]/[action]";
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
