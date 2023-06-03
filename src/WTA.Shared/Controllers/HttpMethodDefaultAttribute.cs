using Microsoft.AspNetCore.Mvc.Routing;

namespace WTA.Shared.Controllers;

public class HttpMethodDefaultAttribute : HttpMethodAttribute
{
    public HttpMethodDefaultAttribute(IEnumerable<string> httpMethods) : base(httpMethods)
    {
    }
}
