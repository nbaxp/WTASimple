using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WTA.Shared.Extensions;

namespace WTA.Shared.Authentication;

public class CustomClaimsPrincipal : ClaimsPrincipal
{
    private readonly IServiceProvider _serviceProvider;

    public CustomClaimsPrincipal(IServiceProvider serviceProvider, ClaimsPrincipal claimsPrincipal) : base(claimsPrincipal)
    {
        this._serviceProvider = serviceProvider;
    }

    public AuthenticateResult? Result { get; private set; }

    public override bool IsInRole(string role)
    {
        var permissionService = this._serviceProvider.GetService<IAuthenticationService>();
        if (permissionService != null)
        {
            // 优先使用本地验证
            this.Result = permissionService.Authenticate(this.Identity?.Name!, role);
        }
        else
        {
            var configuration = this._serviceProvider.GetRequiredService<IConfiguration>();
            var authServer = configuration.GetValue<string>("AuthServer") ?? throw new ArgumentException($"AuthServer 未配置");
            var url = $"{authServer.TrimEnd('/')}/user/is-in-role";
            var httpClientFactory = this._serviceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient();
            var data = new Dictionary<string, string>
        {
            { "name", this.Identity?.Name! },
            { "role", role },
        };
            var response = client.PostAsync(url, new FormUrlEncodedContent(data)).Result;
            this.Result = response.Content.ReadAsStringAsync().Result.FromJson<AuthenticateResult>()!;
        }
        return this.Result.Succeeded;
    }
}
