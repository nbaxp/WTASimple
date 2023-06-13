using System.ComponentModel.DataAnnotations;
using WTA.Shared.Attributes;
using WTA.Shared.Module;

namespace WTA.Application.Tenants;

[Display(Name = "租户管理"), Order(-1)]
public class TenantModule : BaseModule
{
}
