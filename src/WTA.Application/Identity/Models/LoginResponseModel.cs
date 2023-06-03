using System.Text.Json.Serialization;

namespace WTA.Application.Identity.Models;

public class LoginResponseModel
{
    [JsonPropertyName("token_type")]
    public string TokenType = "Bearer";
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    [JsonPropertyName("expires_in")]
    public long? ExpiresIn { get; set; }
}
