using System.ComponentModel.DataAnnotations;

namespace WTA.Application.Identity.Models;

public class LoginRequestModel
{
    [UIHint("select")]
    public string? TenantId { get; set; } = null!;

    [RegularExpression(".{4,8}")]
    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; }
}
