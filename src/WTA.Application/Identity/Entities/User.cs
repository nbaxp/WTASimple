using System.ComponentModel.DataAnnotations;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities;

[Display(Name = "用户")]
public class User : BaseEntity
{
    public string UserName { get; set; } = null!;

    [ScaffoldColumn(false)]
    public string NormalizedUserName { get; set; } = null!;

    public string Name { get; set; } = null!;

    [ScaffoldColumn(false)]
    public string SecurityStamp { get; set; } = null!;

    [ScaffoldColumn(false)]
    public string PasswordHash { get; set; } = null!;

    public bool IsSystem { get; set; }
    public int AccessFailedCount { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
