using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionDebugger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTA.Shared.Attributes;
using WTA.Shared.Domain;
using WTA.Shared.Extensions;
using WTA.Shared.Tenants;

namespace WTA.Shared.Data;

public abstract class BaseDbContext<T> : DbContext where T : DbContext
{
    public static readonly ILoggerFactory DefaultLoggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

    private readonly string _tablePrefix;
    private readonly TenantsOptions _tenantsOptions;
    public string? _tenantId;
    public bool DisableTenantFilter { get; set; }
    public bool DisableSoftDeleteFilter { get; set; }

    public BaseDbContext(DbContextOptions<T> options) : base(options)
    {
        this._tablePrefix = this.GetTablePrefix();
        this._tenantsOptions = this.GetService<IOptions<TenantsOptions>>().Value;
        this._tenantId = this.GetService<ITenantService>()?.GetTenantId();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLoggerFactory(DefaultLoggerFactory);
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //默认配置
        WebApp.Current.ModuleTypes.Where(o => o.Value.ContainsKey(this.GetType()))
            .Select(o => o.Value.GetValueOrDefault(this.GetType()))
            .Where(o => o != null)
        .ForEach(o => o!.ForEach(entityType =>
        {
            var entityTypeBuilder = modelBuilder.Entity(entityType);
            //实体
            if (entityType.IsAssignableTo(typeof(BaseEntity)))
            {
                //租户过滤
                entityTypeBuilder.HasQueryFilter(CreateTenantFilter(entityType, entityTypeBuilder));
                //软删除
                entityTypeBuilder.HasQueryFilter(CreateSoftDeleteFilter(entityType, entityTypeBuilder));
                //主键
                entityTypeBuilder.HasKey(nameof(BaseEntity.Id));
                entityTypeBuilder.Property(nameof(BaseEntity.Id)).ValueGeneratedNever();

                //行版本号
                entityTypeBuilder.Property(nameof(BaseEntity.ConcurrencyStamp)).ValueGeneratedNever();
                //扩展属性
                entityTypeBuilder.Property<Dictionary<string, string>>(nameof(BaseEntity.Properties)).
                HasConversion(v => v.ToJson(), v => v.FromJson<Dictionary<string, string>>()!, DictionaryValueComparer);
                //表名
                entityTypeBuilder.ToTable($"{this._tablePrefix}{entityTypeBuilder.Metadata.GetTableName()}");
                //属性
                entityTypeBuilder.Metadata.GetProperties().ForEach(prop =>
                {
                    if (prop.PropertyInfo != null)
                    {
                        //列注释
                        entityTypeBuilder.Property(prop.Name).HasComment(prop.PropertyInfo?.GetDisplayName());
                        if (prop.PropertyInfo!.PropertyType.IsEnum)
                        {
                            //枚举存为字符串
                            entityTypeBuilder.Property(prop.Name).HasConversion<string>();
                        }
                    }
                });
                //TreeEntity
                if (entityType.IsAssignableTo(typeof(BaseTreeEntity<>).MakeGenericType(entityType)))
                {
                    entityTypeBuilder.HasOne(nameof(BaseTreeEntity<BaseEntity>.Parent))
                        .WithMany(nameof(BaseTreeEntity<BaseEntity>.Children))
                        .HasForeignKey(new string[] { nameof(BaseTreeEntity<BaseEntity>.ParentId) })
                        .OnDelete(DeleteBehavior.NoAction);
                    entityTypeBuilder.Property(nameof(BaseTreeEntity<BaseEntity>.Name)).IsRequired();
                    entityTypeBuilder.Property(nameof(BaseTreeEntity<BaseEntity>.Number)).IsRequired().HasMaxLength(64);
                    entityTypeBuilder.Property(nameof(BaseTreeEntity<BaseEntity>.InternalPath)).IsRequired();
                    entityTypeBuilder.HasIndex(nameof(BaseTreeEntity<BaseEntity>.Number)).IsUnique();
                }
            }
            else if (entityType.IsAssignableTo(typeof(BaseViewEntity)))
            {
                //视图
                entityTypeBuilder.HasNoKey().ToView($"{this._tablePrefix}{entityType.Name}");
            }
        }));

        //自定义配置
        var applyEntityConfigurationMethod = typeof(ModelBuilder)
            .GetMethods()
            .Single(
                e => e.Name == nameof(ModelBuilder.ApplyConfiguration)
                    && e.ContainsGenericParameters
                    && e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition()
                    == typeof(IEntityTypeConfiguration<>));
        if (WebApp.Current.DbConfigTypes.TryGetValue(this.GetType(), out var configTypes))
        {
            configTypes.ForEach(configType =>
            {
                var interfaces = configType.GetInterfaces()
                         .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                         .ToList();
                foreach (var item in interfaces)
                {
                    var entityType = item.GetGenericArguments()[0];
                    var entityTypeBuilder = modelBuilder.GetType().GetMethods()
                        .FirstOrDefault(o => o.Name == "Entity" && o.IsGenericMethod)?
                        .MakeGenericMethod(new Type[] { entityType })
                        .Invoke(modelBuilder, Array.Empty<object>());
                    applyEntityConfigurationMethod.MakeGenericMethod(entityType).Invoke(modelBuilder, new[] { Activator.CreateInstance(configType) });
                }
            });
        }
    }

    private LambdaExpression CreateTenantFilter(Type entityType, EntityTypeBuilder entityTypeBuilder)
    {
        var tenantProperty = entityTypeBuilder.Metadata.FindProperty(nameof(BaseEntity.TenantId));
        var parameter = Expression.Parameter(entityType, "p");
        var left = Expression.Property(parameter, tenantProperty!.PropertyInfo!);
        Expression<Func<string>> expression = () => this._tenantId!;
        var right = expression.Body;
        var filter = Expression.Lambda(Expression.OrElse(
            Expression.Equal(() => this._tenantsOptions.IsEnabled && !this._tenantsOptions.DatabasePerTenant && !this.DisableTenantFilter, () => true),
            Expression.Equal(left, right)),
            parameter);
        Debug.WriteLine(filter.ToScript());
        return filter;
    }

    private LambdaExpression CreateSoftDeleteFilter(Type entityType, EntityTypeBuilder entityTypeBuilder)
    {
        var tenantProperty = entityTypeBuilder.Metadata.FindProperty(nameof(BaseEntity.IsDeleted));
        var parameter = Expression.Parameter(entityType, "p");
        var left = Expression.Property(parameter, tenantProperty!.PropertyInfo!);
        Expression<Func<bool>> expression = () => false;
        var right = expression.Body;
        var filter = Expression.Lambda(Expression.OrElse(
            Expression.Equal(() => !this.DisableSoftDeleteFilter, () => true),
            Expression.Equal(left, right)),
            parameter);
        Debug.WriteLine(filter.ToScript());
        return filter;
    }

    public override int SaveChanges()
    {
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private static readonly ValueComparer<Dictionary<string, string>> DictionaryValueComparer = new((v1, v2) => v1 != null && v2 != null && v1.SequenceEqual(v2),
                o => o.GetHashCode());

    private string GetTablePrefix()
    {
        var prefix = this.GetType().GetCustomAttributes()
                       .Where(o => o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition() == typeof(ModuleAttribute<>))
                       .Select(o => o as ITypeAttribute)
                       .Select(o => o!.Type.Name)
                       .FirstOrDefault()?
                       .TrimEnd("Module");

        if (!string.IsNullOrEmpty(prefix))
        {
            prefix = $"{prefix}_";
        }
        return prefix ?? "";
    }
}
