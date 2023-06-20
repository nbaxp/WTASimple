using System.ComponentModel.DataAnnotations;
using WTA.Shared.Attributes;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities.SystemManagement;

[Hidden]
[Display(Name = "角色权限")]
[SystemManagement]
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
    public bool EnableColumnLimit { get; internal set; }
    public List<string> Columns { get; set; } = new List<string>();
    public bool EnableRowLimit { get; set; }
    public List<string> Rows { get; set; } = new List<string>();
}
