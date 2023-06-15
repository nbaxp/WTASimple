using Microsoft.Extensions.Logging;
using WTA.Application.Identity.Entities;
using WTA.Shared.Controllers;
using WTA.Shared.Data;

namespace WTA.Application.Identity.Controllers;

public class ConnectionStringController : GenericController<ConnectionString, ConnectionString, ConnectionString, ConnectionString, ConnectionString, ConnectionString>
{
    public ConnectionStringController(ILogger<ConnectionString> logger, IRepository<ConnectionString> repository) : base(logger, repository)
    {
        this.Repository.DisableTenantFilter();
    }
}
