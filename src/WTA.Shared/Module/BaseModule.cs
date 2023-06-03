using Microsoft.AspNetCore.Builder;

namespace WTA.Shared.Module;

public abstract class BaseModule
{
    public virtual void ConfigureServices(WebApplicationBuilder builder)
    {
    }

    public virtual void Configure(WebApplication app)
    {
    }
}
