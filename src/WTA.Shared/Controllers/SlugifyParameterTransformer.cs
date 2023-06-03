using Microsoft.AspNetCore.Routing;
using WTA.Shared.Extensions;

namespace WTA.Shared.Controllers;

public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        return value?.ToString()?.ToSlugify();
    }
}
