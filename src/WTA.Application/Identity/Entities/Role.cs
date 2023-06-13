using System.ComponentModel.DataAnnotations;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities;

[Display(Name = "角色")]
public class Role : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Number { get; set; } = null!;
    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
