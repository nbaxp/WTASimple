using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WTA.Shared.Domain;

namespace WTA.Shared.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ConfigTenant(this ModelBuilder builder, string? tenant)
    {
        foreach (var entity in builder.Model.GetEntityTypes().Where(o => o.ClrType.IsAssignableTo(typeof(BaseEntity))).ToList())
        {
            var tenantProperty = entity.FindProperty(nameof(BaseEntity.TenantId));
            var parameter = Expression.Parameter(entity.ClrType, "p");
            var left = Expression.Property(parameter, tenantProperty!.PropertyInfo!);
            Expression<Func<string>> tenantExpression = () => tenant!;
            var right = tenantExpression.Body;
            var filter = Expression.Lambda(Expression.Equal(left, right), parameter);
            builder.Entity(entity.ClrType).HasQueryFilter(filter);
        }
        return builder;
    }
}
