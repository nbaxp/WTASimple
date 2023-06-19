using WTA.Shared.Attributes;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities;

[Order(2)]
public class Role : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Number { get; set; } = null!;
    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
