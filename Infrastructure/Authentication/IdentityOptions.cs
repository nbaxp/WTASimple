namespace WTA.Infrastructure.Authentication;

public class IdentityOptions
{
    public const string Position = "Identity";
    public string Issuer { get; set; } = "value";
    public string Audience { get; set; } = "value";
    public string Key { get; set; } = "0123456789abcdef0123456789abcdef";
    public TimeSpan AccessTokenExpires { get; set; } = TimeSpan.FromMinutes(10);
    public TimeSpan RefreshTokenExpires { get; set; } = TimeSpan.FromDays(14);
    public bool SupportsUserLockout { get; set; } = true;
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public TimeSpan DefaultLockoutTimeSpan { get; set; } = TimeSpan.FromMinutes(10);
}