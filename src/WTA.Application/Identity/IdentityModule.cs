using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using WTA.Application.Identity.Entities;
using WTA.Shared;
using WTA.Shared.Module;

namespace WTA.Application.Identity;

[Display(Name = "系统管理")]
public class IdentityModule : BaseModule
{
    public override void Configure(WebApplication app)
    {
        WebApp.Current.UseScheduler<JobItem>(app);
    }
}
