using Microsoft.Extensions.Logging;
using WTA.Application.Identity.Entities.SystemManagement;
using WTA.Shared.Controllers;
using WTA.Shared.Data;

namespace WTA.Application.Identity.Controllers;

public class PermissionController : GenericController<Permission, Permission, Permission, Permission, Permission, Permission>
{
    public PermissionController(ILogger<Permission> logger, IRepository<Permission> repository) : base(logger, repository)
    {
        this.Repository.DisableTenantFilter();
    }
}
