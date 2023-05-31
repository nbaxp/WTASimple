using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WTA.Application.Domain;
using WTA.Application.Identity.Domain;
using WTA.Infrastructure.Attributes;
using WTA.Infrastructure.Extensions;

namespace WTA.Infrastructure.Data.Config;

[DbContext<IdentityDbContext>]
public class IdentityConfiguration : IEntityTypeConfiguration<Department>,
    IEntityTypeConfiguration<User>,
     IEntityTypeConfiguration<Role>,
     IEntityTypeConfiguration<Permission>,
     IEntityTypeConfiguration<UserRole>,
     IEntityTypeConfiguration<RolePermission>,
     IEntityTypeConfiguration<TenantItem>,
     IEntityTypeConfiguration<JobItem>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
    }

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(o => o.UserName).IsUnique();
        builder.HasIndex(o => o.NormalizedUserName).IsUnique();
    }

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(o => o.Name).IsUnique();
        builder.HasIndex(o => o.Number).IsUnique();
    }

    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.Property(o => o.Columns).HasConversion(p => p.ToJson(), p => p.FromJson<Dictionary<string, string>>()!);
    }

    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasOne(o => o.User).WithMany(o => o.UserRoles).HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(o => o.Role).WithMany(o => o.UserRoles).HasForeignKey(o => o.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasAlternateKey(o => new { o.UserId, o.RoleId });
    }

    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasOne(o => o.Role).WithMany(o => o.RolePermissions).HasForeignKey(o => o.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(o => o.Permission).WithMany(o => o.RolePermissions).HasForeignKey(o => o.PermissionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasAlternateKey(o => new { o.RoleId, o.PermissionId });
        builder.Property(o => o.Columns).HasConversion(p => p.ToJson(), p => p.FromJson<List<string>>()!);
        builder.Property(o => o.Rows).HasConversion(p => p.ToJson(), p => p.FromJson<List<string>>()!);
    }

    public void Configure(EntityTypeBuilder<JobItem> builder)
    {
    }

    public void Configure(EntityTypeBuilder<TenantItem> builder)
    {
    }
}