using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using WTA.Application.Domain;
using WTA.Application.Identity.Domain;
using WTA.Infrastructure;
using WTA.Infrastructure.Application;
using WTA.Infrastructure.Attributes;
using WTA.Infrastructure.Data;
using WTA.Infrastructure.Extensions;
using WTA.Infrastructure.Identity;

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
        //部门初始化
        context.Set<Department>().Add(
            new Department
            {
                IsReadonly = true,
                Name = "企业",
                Number = "Enterprise",
                Children = new List<Department>
                {
                    new Department {
                        Name="部门",
                        Number="Location",
                    }
                }
            }.UpdatePath());

        // 权限初始化
        InitPermissions(context);

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

    private void InitPermissions(IdentityDbContext context)
    {
        //var controllerFeature = new ControllerFeature();
        //this._partManager.PopulateFeature(controllerFeature);
        //var controllerTypeInfos = controllerFeature.Controllers;
        var actionDescriptors = this._actionProvider.ActionDescriptors.Items;

        context.Set<Permission>().Add(new Permission
        {
            Type = PermissionType.Resource,
            Name = "首页",
            Number = "",
            Path = "/",
            Redirect = "home",
            Icon = "home",
            Order = -1,
        }.UpdatePath());

        WebApp.Current.ModuleTypes.ForEach(m =>
        {
            var moduleType = m.Key;
            var modulePermission = new Permission
            {
                Type = PermissionType.Module,
                Name = moduleType.GetDisplayName(),
                Number = moduleType.Name,
                Path = $"/{moduleType.Name.TrimEnd("Module").ToSlugify()}",
                Icon = moduleType.GetCustomAttribute<IconAttribute>()?.Icon,
                Order = moduleType.GetCustomAttribute<OrderAttribute>()?.Order
            };
            m.Value.SelectMany(o => o.Value).ForEach(entityType =>
            {
                var columns = entityType.GetProperties()
                    //.Where(o => o.PropertyType.IsValueType || o.PropertyType == typeof(string))
                    .OrderBy(o => o.GetCustomAttribute<DisplayAttribute>()?.GetOrder())
                    .ToDictionary(o => o.Name, o => o.GetDisplayName());
                var resourcePermission = new Permission
                {
                    IsReadonly = true,
                    Type = PermissionType.Resource,
                    Name = entityType.GetDisplayName(),
                    Number = $"{modulePermission.Number}.{entityType.Name}",
                    Path = $"{entityType.Name.ToSlugify()}",
                    Icon = entityType.GetCustomAttribute<IconAttribute>()?.Icon,
                    Order = entityType.GetCustomAttribute<OrderAttribute>()?.Order,
                    Columns = columns
                };

                var groupAttribute = entityType.GetCustomAttributes().FirstOrDefault(o => o.GetType().IsAssignableTo(typeof(GroupAttribute)));
                if (groupAttribute != null)
                {
                    var groupNumber = groupAttribute.GetType().Name;
                    var groupPermission = modulePermission.Children.FirstOrDefault(o => o.Number == groupNumber);
                    if (groupPermission == null)
                    {
                        groupPermission = new Permission
                        {
                            Type = PermissionType.Group,
                            Name = groupAttribute.GetType().GetDisplayName(),
                            Number = groupNumber,
                            Path = $"{groupAttribute.GetType().Name.TrimEnd("Attribute").ToSlugify()}",
                            Icon = groupAttribute.GetType().GetCustomAttribute<IconAttribute>()?.Icon,
                            Order = groupAttribute.GetType().GetCustomAttribute<OrderAttribute>()?.Order
                        };
                        modulePermission.Children.Add(groupPermission);
                    }
                    groupPermission.Children.Add(resourcePermission);
                }
                else
                {
                    modulePermission.Children.Add(resourcePermission);
                }

                var resourceServiceType = typeof(IResourceService<>).MakeGenericType(entityType);
                actionDescriptors
                .Select(o => o as ControllerActionDescriptor)
                .Where(o => o != null && o.ControllerTypeInfo.AsType().IsAssignableTo(resourceServiceType))
                .ForEach(actionDescriptor =>
                {
                    var operation = actionDescriptor?.ActionName!;
                    var methodInfo = actionDescriptor?.MethodInfo!;
                    resourcePermission.Children.Add(new Permission
                    {
                        IsReadonly = true,
                        Type = PermissionType.Operation,
                        Name = methodInfo.GetDisplayName(),
                        Number = $"{methodInfo.ReflectedType!.FullName}.{operation}",
                        Path = $"{operation.TrimEnd("Async").ToSlugify()}",
                        Icon = methodInfo.GetCustomAttribute<IconAttribute>()?.Icon,
                        Order = methodInfo.GetCustomAttribute<OrderAttribute>()?.Order
                    });
                });
            });
            modulePermission.UpdatePath();
            context.Set<Permission>().Add(modulePermission);
            context.SaveChanges();
        });
    }
}