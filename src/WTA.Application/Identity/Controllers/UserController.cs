using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WTA.Application.Identity.Entities;
using WTA.Shared.Attributes;
using WTA.Shared.Authentication;
using WTA.Shared.Controllers;
using WTA.Shared.Data;
using WTA.Shared.Extensions;

namespace WTA.Application.Identity.Controllers;

[Implement<IAuthenticationService>]
[ApiExplorerSettings(GroupName = nameof(IdentityModule))]
public class UserController : GenericController<User, User, User, User, User, User>, IAuthenticationService
{
    public UserController(ILogger<User> logger, IRepository<User> repository) : base(logger, repository)
    {
    }

    [HttpPost, Hidden]
    public AuthenticateResult Authenticate(string name, string operation)
    {
        var query = this.Repository.AsNoTracking();

        var result = new AuthenticateResult
        {
            Succeeded = query.Any(o => o.UserName == name && o.UserRoles.Any(o => o.Role.RolePermissions.Any(o => o.Permission.Type == PermissionType.Operation && o.Permission.Number == operation)))
        };
        if (result.Succeeded)
        {
            var rolePermissions = query
                .Where(o => o.UserName == name)
                .SelectMany(o => o.UserRoles)
                .Select(o => o.Role)
                .SelectMany(o => o.RolePermissions)
                .Where(o => o.Permission.Children.Any(p => p.Number == operation))
                .ToList();
            result.EnableColumnLimit = rolePermissions.Any(o => o.EnableColumnLimit);
            if (result.EnableColumnLimit)
            {
                result.Columns = rolePermissions.Where(o => o.EnableColumnLimit).SelectMany(o => o.Columns).Distinct().ToList();
            }
            result.EnableRowLimit = rolePermissions.Any(o => o.EnableRowLimit);
            if (result.EnableRowLimit)
            {
                result.Rows = rolePermissions.Where(o => o.EnableColumnLimit).SelectMany(o => o.Rows).ToList();
            }
        }
        return result;
    }

    [HttpPost, Hidden]
    [Display(Name = "用户信息")]
    public User? Info()
    {
        var user = this.Repository
            .AsNoTracking()
            .Include(o => o.Department)
            .Include(o => o.UserRoles)
            .ThenInclude(o => o.Role)
            .ThenInclude(o => o.RolePermissions)
            .ThenInclude(o => o.Permission)
            .FirstOrDefault(o => o.UserName == this.User.Identity!.Name);
        if (user != null)
        {
            user.SecurityStamp = string.Empty;
            user.PasswordHash = string.Empty;
        }
        return user!;
    }
}
