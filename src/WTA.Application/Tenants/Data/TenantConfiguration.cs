using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WTA.Application.Tenants.Entities;
using WTA.Shared.Attributes;

namespace WTA.Shared.Data.Config;

[DbContext<TenantDbContext>]
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>,
    IEntityTypeConfiguration<ConnectionString>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasIndex(o => o.Name).IsUnique();
        builder.HasIndex(o => o.Number).IsUnique();
    }

    public void Configure(EntityTypeBuilder<ConnectionString> builder)
    {
        builder.HasOne(o => o.Parent).WithMany(o => o.ConnectionStrings).HasForeignKey(o => o.ParentId).OnDelete(DeleteBehavior.Cascade);
    }
}
