using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WTA.Application.Identity.Entities;
using WTA.Application.Monitor.Entities;
using WTA.Shared.Extensions;

namespace WTA.Shared.Data.Config;

public class IdentityConfiguration : IDbConfig<IdentityDbContext>, IEntityTypeConfiguration<Tenant>,
    IEntityTypeConfiguration<ConnectionString>,
    IEntityTypeConfiguration<Department>,
    IEntityTypeConfiguration<User>,
     IEntityTypeConfiguration<Role>,
     IEntityTypeConfiguration<Permission>,
     IEntityTypeConfiguration<Post>,
     IEntityTypeConfiguration<UserRole>,
     IEntityTypeConfiguration<RolePermission>,
     IEntityTypeConfiguration<JobItem>
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

    public void Configure(EntityTypeBuilder<Department> builder)
    {
    }

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasOne(o => o.Post).WithMany(o => o.Users).HasForeignKey(o => o.PostId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(o => o.Department).WithMany(o => o.Users).HasForeignKey(o => o.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasAlternateKey(o => o.UserName);
        builder.HasIndex(o => o.NormalizedUserName).IsUnique();
    }

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasAlternateKey(o => o.Number);
    }

    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.Property(o => o.Type).IsRequired();
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

    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasAlternateKey(o => o.Number);
    }
}
