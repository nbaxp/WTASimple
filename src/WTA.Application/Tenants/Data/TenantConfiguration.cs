using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WTA.Application.Tenants.Entities;
using WTA.Shared.Attributes;

namespace WTA.Shared.Data.Config;

[DbContext<IdentityDbContext>]
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>,
    IEntityTypeConfiguration<ConnectionString>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.Property(o => o.Name).IsRequired();
        builder.HasIndex(o => o.Name).IsUnique();
        builder.Property(o => o.Number).IsRequired();
        builder.HasIndex(o => o.Number).IsUnique();
    }

    public void Configure(EntityTypeBuilder<ConnectionString> builder)
    {
        builder.HasOne(o => o.Tenant).WithMany(o => o.ConnectionStrings).HasForeignKey(o => o.ParentId).OnDelete(DeleteBehavior.Cascade);
    }
}
