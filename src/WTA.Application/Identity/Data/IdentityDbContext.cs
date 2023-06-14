
using Microsoft.EntityFrameworkCore;
using WTA.Application.Identity;
using WTA.Shared.Attributes;

namespace WTA.Shared.Data;

[Module<IdentityModule>, IgnoreMultiTenancy]
public class IdentityDbContext : BaseDbContext<IdentityDbContext>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }
}
