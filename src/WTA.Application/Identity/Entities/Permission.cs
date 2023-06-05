using WTA.Shared.Domain;

namespace WTA.Application.Identity.Entities;

public class Permission : BaseTreeEntity<Permission>
{
    public PermissionType Type { get; set; }
    public bool IsHidden { get; set; }
    public bool IsExternal { get; set; }
    public string? Path { get; set; }
    public string? Redirect { get; set; }
    public string? Icon { get; set; }
    public bool IsSystem { get; set; }
    public Dictionary<string, string> Columns { get; set; } = new Dictionary<string, string>();
    public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}