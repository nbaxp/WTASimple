using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using WTA.Shared.Domain;

namespace WTA.Shared.Controllers;

public class BaseController : Controller
{
    public BaseController()
    {
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var descriptor = (context.ActionDescriptor as ControllerActionDescriptor)!;
        if (!descriptor.MethodInfo.CustomAttributes.Any(o => o.AttributeType == typeof(AllowAnonymousAttribute)))
        {
            var operaation = $"{descriptor.ControllerName}.{descriptor.ActionName}";
            if (!this.HttpContext.User.Identity!.IsAuthenticated)
            {
                context.Result = this.Unauthorized();
            }
            else if (!context.HttpContext.User.IsInRole(operaation))
            {
                context.Result = this.Forbid();
            }
        }
        context.ModelState.Remove(nameof(BaseEntity.CreatedOn));
        context.ModelState.Remove(nameof(BaseEntity.ConcurrencyStamp));
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
