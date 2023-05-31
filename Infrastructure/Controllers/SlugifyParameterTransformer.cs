using WTA.Infrastructure.Extensions;

namespace WTA.Infrastructure.Controllers;

public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        return value?.ToString()?.ToSlugify();
    }
}