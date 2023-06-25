using System.ComponentModel.DataAnnotations;
using System.Reflection;
using LinqToDB.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using WTA.Application.Identity.Entities.SystemManagement;
using WTA.Application.Identity.Entities.Tenants;
using WTA.Application.Monitor.Entities;
using WTA.Shared;
using WTA.Shared.Application;
using WTA.Shared.Attributes;
using WTA.Shared.Data;
using WTA.Shared.Extensions;
using WTA.Shared.Identity;

namespace WTA.Application.Identity.Data;

public class IdentityDbSeed : IDbSeed<IdentityDbContext>
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IActionDescriptorCollectionProvider _actionProvider;

    public IdentityDbSeed(IPasswordHasher passwordHasher, IActionDescriptorCollectionProvider actionProvider)
    {
        this._passwordHasher = passwordHasher;
        this._actionProvider = actionProvider;
    }

    public void Seed(IdentityDbContext context)
    {
        //禁用多租户和软删除过滤器
        context.DisableTenantFilter = true; ;
        context.DisableSoftDeleteFilter = true;

        // 定时器初始化
        context.Set<JobItem>().Add(new JobItem { Name = "客户端监测", Cron = "* * * * *", Service = "WTA.Application.Monitor.UserLoginSrevice" });

        // 数据字典初始化
        InitDictionaries(context);

        //租户初始化
        var tenant = new Tenant
        {
            Name = "默认",
            Number = "default",
            DataBaseCreated = false,
            ConnectionStrings = new List<ConnectionString>
            {
                new ConnectionString(){ Name="Identity",Value="Data Source=data2.db" }
            }
        }.SetIdBy(o => o.Number);
        context.Set<Tenant>().Add(tenant);
        var defaultTenantId = tenant.Id;

        // 权限初始化
        InitPermissions(context);

        //部门初始化
        context.Set<Department>().Add(
            new Department
            {
                Name = "企业",
                Number = "Enterprise",
                Children = new List<Department>
                {
                    new Department {
                        Name="总经办",
                        Number="Root",
                        Children=new List<Department>
                        {
                            new Department
                            {
                                Name="部门",
                                Number="Department"
                            }
                        }
                    }
                }
            }.UpdateId()
            .UpdatePath());

        // 角色初始化
        var superRole = new Role
        {
            IsReadonly = true,
            Name = "超级管理员角色",
            Number = "super",
        }.SetIdBy(o => o.Number);
        context.Set<Permission>().ToList().ForEach(o => superRole.RolePermissions.Add(new RolePermission
        {
            IsReadonly = true,
            RoleId = superRole.Id,
            PermissionId = o.Id,
            EnableColumnLimit = false,
            Columns = o.Columns.Keys.ToList(),
            // EnableRowLimit = true,
            // RowLimit = "CreatorId = {User.Id}"
        }.SetIdBy(o => new { o.RoleId, o.PermissionId })));
        context.Set<Role>().Add(superRole);
        context.SaveChanges();

        // 用户初始化
        var superUser = new User
        {
            IsReadonly = true,
            UserName = "super",
            NormalizedUserName = "super".Normalize(),
            Name = "超级管理员",
            SecurityStamp = "123456",
            PasswordHash = this._passwordHasher.HashPassword("123456", "123456"),
            Properties = new Dictionary<string, string> { { "key1", "value1" } }
        }.SetIdBy(o => o.UserName);
        superUser.UserRoles.Add(new UserRole { IsReadonly = true, UserId = superUser.Id, RoleId = superRole.Id }.SetIdBy(o => new { o.UserId, o.RoleId }));
        context.Set<User>().Add(superUser);
    }

    private void InitDictionaries(DbContext context)
    {
    }

    private void InitPermissions(DbContext context)
    {
        //var controllerFeature = new ControllerFeature();
        //this._partManager.PopulateFeature(controllerFeature);
        //var controllerTypeInfos = controllerFeature.Controllers;
        var actionDescriptors = this._actionProvider.ActionDescriptors.Items;

        context.Set<Permission>().Add(new Permission
        {
            IsReadonly = true,
            Type = PermissionType.Resource,
            Name = "首页",
            Number = "home",
            Path = "home",
            Method = "POST",
            Component = "home",
            Icon = "home",
            Order = -3,
        }.UpdateId().UpdatePath());
        context.Set<Permission>().Add(new Permission
        {
            IsExternal = true,
            IsReadonly = true,
            Type = PermissionType.Resource,
            Name = "帮助",
            Number = "help",
            Path = "https://element-plus.org/",
            Method = "GET",
            Icon = "ep-link",
            Order = 1000,
        }.UpdateId().UpdatePath());

        WebApp.Current.Assemblies.SelectMany(o => o.GetTypes()).Where(o => o.IsClass && !o.IsAbstract && o.IsAssignableTo(typeof(IResource))).ForEach(resourceType =>
        {
            // 获取列
            var columns = resourceType.GetProperties()
                    //.Where(o => o.PropertyType.IsValueType || o.PropertyType == typeof(string))
                    .OrderBy(o => o.GetCustomAttribute<DisplayAttribute>()?.GetOrder())
                    .ToDictionary(o => o.Name, o => o.GetDisplayName());
            // 创建资源权限
            var resourcePermission = new Permission
            {
                IsReadonly = true,
                Type = PermissionType.Resource,
                Name = resourceType.GetDisplayName(),
                Number = resourceType.Name,
                Path = resourceType.Name.ToSlugify(),
                Component = resourceType.GetCustomAttribute<ComponentAttribute>()?.Component,
                IsHidden = resourceType.HasAttribute<HiddenAttribute>(),
                Icon = resourceType.GetCustomAttribute<IconAttribute>()?.Icon ?? IconAttribute.File,
                Order = resourceType.GetCustomAttribute<OrderAttribute>()?.Order ?? OrderAttribute.Default,
                Columns = columns
            }.UpdateId();
            // 创建按钮权限
            var resourceServiceType = typeof(IResourceService<>).MakeGenericType(resourceType);
            actionDescriptors
            .Select(o => o as ControllerActionDescriptor)
            .Where(o => o != null && o.ControllerTypeInfo.AsType().IsAssignableTo(resourceServiceType))
            .ForEach(actionDescriptor =>
            {
                var operation = actionDescriptor?.ActionName!;
                var methodInfo = actionDescriptor?.MethodInfo!;
                var method = (methodInfo.GetCustomAttributes().FirstOrDefault(o => o.GetType().IsAssignableTo(typeof(HttpMethodAttribute)))
                as HttpMethodAttribute)?.HttpMethods?.FirstOrDefault() ?? "POST";
                if (method != "GET")
                {
                    resourcePermission.Children.Add(new Permission
                    {
                        IsReadonly = true,
                        Type = PermissionType.Operation,
                        Name = methodInfo.GetDisplayName(),
                        Number = $"{actionDescriptor?.ControllerName}.{operation}",
                        Path = $"{operation.TrimEnd("Async").ToSlugify()}",
                        IsHidden = methodInfo.GetCustomAttributes<HiddenAttribute>().Any(),
                        Method = method,
                        Icon = methodInfo.GetCustomAttribute<IconAttribute>()?.Icon ?? $"{operation.TrimEnd("Async").ToSlugify()}",
                        Order = methodInfo.GetCustomAttribute<OrderAttribute>()?.Order ?? OrderAttribute.Default,
                        HtmlClass = methodInfo.GetCustomAttribute<HtmlClassAttribute>()?.Class ?? HtmlClassAttribute.Default,
                        IsTop = methodInfo.GetCustomAttribute<MultipleAttribute>() != null
                    }.UpdateId());
                }
            });
            // 实体分组
            var groupAttribute = resourceType.GetCustomAttributes().FirstOrDefault(o => o.GetType().IsAssignableTo(typeof(GroupAttribute)));
            if (groupAttribute != null)
            {
                var groupNumber = groupAttribute.GetType().Name;
                var groupPermission = context.Set<Permission>().FirstOrDefault(o => o.Number == groupAttribute.GetType().Name);
                groupPermission ??= new Permission
                {
                    Type = PermissionType.Group,
                    Name = groupAttribute.GetType().GetDisplayName(),
                    Number = groupNumber,
                    Path = $"{groupAttribute.GetType().Name.TrimEnd("Attribute").ToSlugify()}",
                    IsHidden = groupAttribute.GetType().HasAttribute<HiddenAttribute>(),
                    Icon = groupAttribute.GetType().GetCustomAttribute<IconAttribute>()?.Icon ?? IconAttribute.Folder,
                    Order = groupAttribute.GetType().GetCustomAttribute<OrderAttribute>()?.Order ?? OrderAttribute.Default
                }.UpdateId();
                groupPermission.Children.Add(resourcePermission);
                var moduleType = groupAttribute.GetType().GetCustomAttributes()
                       .Where(o => o.GetType().IsGenericType && o.GetType().GetGenericTypeDefinition() == typeof(ModuleAttribute<>))
                       .Select(o => o as ITypeAttribute).Select(o => o?.Type).FirstOrDefault();
                if (moduleType != null)
                {
                    var modulePermission = context.Set<Permission>().FirstOrDefault(o => o.Number == moduleType.Name);
                    if (modulePermission == null)
                    {
                        modulePermission = new Permission
                        {
                            Type = PermissionType.Module,
                            Name = moduleType.GetDisplayName(),
                            Number = moduleType.Name,
                            Path = moduleType.Name.TrimEnd("Module").ToSlugify(),
                            IsHidden = moduleType.HasAttribute<HiddenAttribute>(),
                            Icon = moduleType.GetCustomAttribute<IconAttribute>()?.Icon ?? IconAttribute.Folder,
                            Order = moduleType.GetCustomAttribute<OrderAttribute>()?.Order ?? OrderAttribute.Default
                        }.UpdateId();
                        context.Set<Permission>().Add(modulePermission.UpdatePath());
                    }
                    modulePermission.Children.Add(groupPermission);
                }
                else
                {
                    context.Set<Permission>().AddOrUpdate(groupPermission.UpdatePath());
                }
            }
            else
            {
                context.Set<Permission>().Add(resourcePermission.UpdatePath());
            }
            context.SaveChanges();
        });
    }
}
