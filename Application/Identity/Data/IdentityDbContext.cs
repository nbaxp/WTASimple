using Microsoft.EntityFrameworkCore;
using WTA.Application.Identity;
using WTA.Infrastructure.Attributes;

namespace WTA.Infrastructure.Data;

[Module<IdentityModule>]
public class IdentityDbContext : BaseDbContext
{
    public IdentityDbContext(DbContextOptions options) : base(options)
    {
    }
}