using Microsoft.AspNetCore.Builder;
using WTA.Application.Monitor.Entities;
using WTA.Shared;
using WTA.Shared.Attributes;
using WTA.Shared.Module;

namespace WTA.Application.Identity;

[Order(1)]
public class IdentityModule : BaseModule
{
    public override void Configure(WebApplication app)
    {
        WebApp.Current.UseScheduler<JobItem>(app);
    }
}
