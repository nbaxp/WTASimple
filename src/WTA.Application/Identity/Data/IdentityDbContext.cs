using Microsoft.EntityFrameworkCore;
using WTA.Application.Identity;
using WTA.Shared.Attributes;

namespace WTA.Shared.Data;

[Module<IdentityModule>]
public class IdentityDbContext : BaseDbContext
{
    public IdentityDbContext(DbContextOptions options) : base(options)
    {
    }
}