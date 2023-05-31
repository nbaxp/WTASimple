namespace WTA.Application.Identity.Models;

public class LoginRequestModel
{
    public string? TenantId { get; set; }
    public Guid LocationId { get; set; }
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; }
}