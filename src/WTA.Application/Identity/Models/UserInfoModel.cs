using WTA.Application.Identity.Entities.SystemManagement;

namespace WTA.Application.Identity.Models;

public class UserInfoModel
{
    public User User { get; set; } = null!;
    public List<Permission> Permissions { get; set; } = new List<Permission>();
}
