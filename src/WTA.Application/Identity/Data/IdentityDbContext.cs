using Microsoft.EntityFrameworkCore;
using WTA.Shared.Attributes;

namespace WTA.Shared.Data;

[IgnoreMultiTenancy]
public class IdentityDbContext : BaseDbContext<IdentityDbContext>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }
}
