using System.ComponentModel.DataAnnotations;
using WTA.Shared.Attributes;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities.SystemManagement;

[Hidden]
[Display(Name = "用户角色")]
[SystemManagement]
public class UserRole : BaseEntity
{
    [Required]
    public Guid? UserId { get; set; }
    [Required]
    public Guid? RoleId { get; set; }
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
