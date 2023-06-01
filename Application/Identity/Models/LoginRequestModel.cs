using System.ComponentModel.DataAnnotations;

namespace WTA.Application.Identity.Models;

public class LoginRequestModel
{
    [UIHint("select")]
    [Required]
    public string TenantId { get; set; } = null!;

    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; }
}
