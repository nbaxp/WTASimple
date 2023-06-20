using System.ComponentModel.DataAnnotations;
using WTA.Shared.Attributes;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities.SystemManagement;

[Order(1)]
[SystemManagement]
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

    public int AccessFailedCount { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTime? LockoutEnd { get; set; }

    [Navigation]
    public Guid? DepartmentId { get; set; }

    public Guid? PostId { get; set; }

    public Department? Department { get; set; }
    public Post? Post { get; set; }

    public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
