using System.ComponentModel.DataAnnotations;
using WTA.Shared.Attributes;
using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities;

[Order(3)]
public class Permission : BaseTreeEntity<Permission>
{
    [Required]
    public PermissionType? Type { get; set; }

    [Required]
    public bool? IsHidden { get; set; } = false;

    [Required]
    public bool? IsExternal { get; set; } = false;

    public string? Path { get; set; }
    public string? Method { get; set; }
    public string? Component { get; set; }
    public string? Redirect { get; set; }
    public string? Icon { get; set; }
    public string? HtmlClass { get; set; }

    [Required]
    public bool? IsTop { get; set; } = false;

    public Dictionary<string, string> Columns { get; set; } = new Dictionary<string, string>();
    public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
