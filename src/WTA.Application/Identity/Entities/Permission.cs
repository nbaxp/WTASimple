using System.ComponentModel.DataAnnotations;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities;

[Display(Name = "权限")]
public class Permission : BaseTreeEntity<Permission>
{
    [Required]
    public PermissionType? Type { get; set; }
    public bool IsHidden { get; set; }
    public bool IsExternal { get; set; }
    public string? Path { get; set; }
    public string? Method { get; set; }
    public string? Component { get; set; }
    public string? Redirect { get; set; }
    public string? Icon { get; set; }
    public bool IsTop { get; set; }
    public bool IsSystem { get; set; }
    public Dictionary<string, string> Columns { get; set; } = new Dictionary<string, string>();
    public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
