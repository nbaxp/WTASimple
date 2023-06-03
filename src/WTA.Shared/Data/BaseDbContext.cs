using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using WTA.Shared.Attributes;
using WTA.Shared.Domain;
using WTA.Shared.Extensions;

namespace WTA.Shared.Data;

public abstract class BaseDbContext<T> : DbContext where T : DbContext
{
    public static readonly ILoggerFactory DefaultLoggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

    public BaseDbContext(DbContextOptions<T> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLoggerFactory(DefaultLoggerFactory);
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region 批量设置

        //批量添加实体
        WebApp.Current.ModuleTypes.Where(o => o.Value.ContainsKey(this.GetType()))
            .Select(o => o.Value.GetValueOrDefault(this.GetType()))
            .Where(o => o != null)
            .ForEach(o => o!.ForEach(t => modelBuilder.Entity(t)));

        var getTablePrefix = (Type contextType) =>
        {
            var prefix = contextType.GetCustomAttributes()
                .Where(o => o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition() == typeof(ModuleAttribute<>))
                .Select(o => o as ITypeAttribute)
                .Select(o => o!.Type.Name)
                .FirstOrDefault()?
                .TrimEnd("Module");

            if (!string.IsNullOrEmpty(prefix))
            {
                prefix = $"{prefix}_";
            }
            return prefix;
        };
        // 批量设置实体
        modelBuilder.Model.GetEntityTypes().Where(o => o.ClrType.IsAssignableTo(typeof(BaseEntity))).ToList().ForEach(item =>
        {
            // 设置Id
            modelBuilder.Entity(item.ClrType).HasKey(nameof(BaseEntity.Id));
            modelBuilder.Entity(item.ClrType).Property(nameof(BaseEntity.Id)).ValueGeneratedNever();
            // 设置行版本号
            modelBuilder.Entity(item.ClrType).Property(nameof(BaseEntity.ConcurrencyStamp)).ValueGeneratedNever();
            // 设置扩展属性
            var vc = new ValueComparer<Dictionary<string, string>>(
                (v1, v2) => v1 != null && v2 != null && v1.SequenceEqual(v2),
                o => o.GetHashCode()
                );
            modelBuilder.Entity(item.ClrType).Property<Dictionary<string, string>>(nameof(BaseEntity.Properties)).
                HasConversion(v => v.ToJson(),
                v => v.FromJson<Dictionary<string, string>>()!,
                vc);
            // 配置表名称和注释
            modelBuilder.Entity(item.ClrType, builder =>
            {
                var prefix = getTablePrefix(this.GetType());
                var tableName = $"{prefix}{item.GetTableName()}";
                builder.ToTable(tableName);
                builder.ToTable(t => t.HasComment(item.ClrType.GetDisplayName()));
                //
                foreach (var prop in item.GetProperties())
                {
                    if (prop.PropertyInfo != null)
                    {
                        builder.Property(prop.Name).HasComment(prop.PropertyInfo?.GetDisplayName());
                        if (prop.PropertyInfo!.PropertyType.IsEnum)
                        {
                            builder.Property(prop.Name).HasConversion<string>();
                        }
                    }
                }
            });
            // 配置 TreeEntity
            if (item.ClrType.IsAssignableTo(typeof(BaseTreeEntity<>).MakeGenericType(item.ClrType)))
            {
                modelBuilder.Entity(item.ClrType).HasOne(nameof(BaseTreeEntity<BaseEntity>.Parent))
                    .WithMany(nameof(BaseTreeEntity<BaseEntity>.Children))
                    .HasForeignKey(new string[] { nameof(BaseTreeEntity<BaseEntity>.ParentId) })
                    .OnDelete(DeleteBehavior.NoAction);
                modelBuilder.Entity(item.ClrType).Property(nameof(BaseTreeEntity<BaseEntity>.Name)).IsRequired();
                modelBuilder.Entity(item.ClrType).Property(nameof(BaseTreeEntity<BaseEntity>.Number)).IsRequired().HasMaxLength(64);
                modelBuilder.Entity(item.ClrType).Property(nameof(BaseTreeEntity<BaseEntity>.InternalPath)).IsRequired();
                modelBuilder.Entity(item.ClrType).HasIndex(nameof(BaseTreeEntity<BaseEntity>.Number)).IsUnique();
            }
        });

        //批量设置视图
        modelBuilder.Model.GetEntityTypes().Where(o => o.ClrType.IsAssignableTo(typeof(BaseViewEntity))).ToList().ForEach(item =>
        {
            var prefix = getTablePrefix(this.GetType());
            var viewName = $"{prefix}{item.ClrType.Name}";
            modelBuilder.Entity(item.ClrType).HasNoKey().ToView(viewName);
        });

        #endregion 批量设置

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
}
