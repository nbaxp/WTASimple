using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WTA.Infrastructure.Controllers;

public class BaseController : Controller
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var descriptor = (context.ActionDescriptor as ControllerActionDescriptor)!;
        if (!descriptor.MethodInfo.CustomAttributes.Any(o => o.AttributeType == typeof(AllowAnonymousAttribute)))
        {
            var operaation = $"{this.GetType().FullName}.{descriptor.ActionName}";
            if (!this.HttpContext.User.Identity!.IsAuthenticated)
            {
                context.Result = this.Unauthorized("未登录");
            }
            else if (!context.HttpContext.User.IsInRole(operaation))
            {
                context.Result = this.Forbid("无权限");
            }
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        //(context.Result as ObjectResult)!.Value = new {
        //    Items = new List<Dictionary<string, object>> { }
        //};
        //((context.Result as ObjectResult).Value.GetType().GetProperty("Items").GetValue((context.Result as ObjectResult).Value) as System.Collections.IList).Clear();
        base.OnActionExecuted(context);
    }
}