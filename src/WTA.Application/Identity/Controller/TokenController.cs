using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WTA.Application.Domain;
using WTA.Application.Identity.Domain;
using WTA.Application.Identity.Models;
using WTA.Shared.Authentication;
using WTA.Shared.Controllers;
using WTA.Shared.Data;
using WTA.Shared.Extensions;
using WTA.Shared.Identity;

namespace WTA.Application.Identity.Controllers;

[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = nameof(IdentityModule))]
public class TokenController : BaseController
{
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly IdentityOptions _identityOptions;
    private readonly IPasswordHasher _passwordHasher;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly SigningCredentials _credentials;
    private readonly IRepository<TenantItem> _tenantRepository;
    private readonly IRepository<User> _userRepository;

    public TokenController(TokenValidationParameters tokenValidationParameters,
        JwtSecurityTokenHandler jwtSecurityTokenHandler,
        SigningCredentials credentials,
        IOptions<IdentityOptions> identityOptions,
        IPasswordHasher passwordHasher,
        IRepository<TenantItem> _tenantRepository,
        IRepository<User> userRepository)
    {
        this._tokenValidationParameters = tokenValidationParameters;
        this._jwtSecurityTokenHandler = jwtSecurityTokenHandler;
        this._credentials = credentials;
        this._identityOptions = identityOptions.Value;
        this._passwordHasher = passwordHasher;
        this._tenantRepository = _tenantRepository;
        this._userRepository = userRepository;
    }

    [HttpGet("[action]")]
    [AllowAnonymous]
    public object Create()
    {
        return typeof(LoginRequestModel).GetMetadataForType();
    }

    [HttpPost("[action]")]
    [AllowAnonymous]
    public IActionResult Create([FromBody] LoginRequestModel model)
    {
        if (this.ModelState.IsValid)
        {
            try
            {
                var additionalClaims = new List<Claim> { new Claim(nameof(model.TenantId), model.TenantId!) };
                if (model.TenantId != null)
                {
                    var tenantQuery = this._tenantRepository.AsNoTracking();
                    var tenantItem = tenantQuery.FirstOrDefault(o => o.TenantId == model.TenantId) ?? throw new Exception("租户不存在");
                    additionalClaims.Add(new Claim(nameof(model.TenantId), model.TenantId?.ToString()!));
                }
                //
                var userQuery = this._userRepository.Queryable();
                var user = userQuery.FirstOrDefault(o => o.UserName == model.UserName);
                if (user != null)
                {
                    if (this._identityOptions.SupportsUserLockout)
                    {
                        if (user.LockoutEnd.HasValue)
                        {
                            if (user.LockoutEnd.Value >= DateTimeOffset.Now)
                            {
                                throw new Exception("用户处于锁定状态");
                            }
                            else
                            {
                                user.AccessFailedCount = 0;
                                user.LockoutEnd = null;
                                this._userRepository.SaveChanges();
                            }
                        }
                    }

                    if (user.PasswordHash != this._passwordHasher.HashPassword(model.Password, user.SecurityStamp!))
                    {
                        user.AccessFailedCount++;
                        this._userRepository.SaveChanges();
                        if (user.AccessFailedCount == this._identityOptions.MaxFailedAccessAttempts)
                        {
                            throw new Exception($"用户已锁定,{(user.LockoutEnd!.Value - DateTime.Now).TotalMinutes} 分钟后解除");
                        }
                        else
                        {
                            throw new Exception($"密码错误,剩余尝试错误次数为 {this._identityOptions.MaxFailedAccessAttempts - user.AccessFailedCount}");
                        }
                    }
                }
                else
                {
                    throw new Exception("用户名或密码错误");
                }
                //
                var roles = this._userRepository.AsNoTracking()
                    .Where(o => o.UserName == model.UserName)
                    .SelectMany(o => o.UserRoles)
                    .Select(o => o.Role.Name)
                    .ToList()
                    .Select(o => new Claim(this._tokenValidationParameters.RoleClaimType, o));
                additionalClaims.AddRange(roles);
                var subject = CreateSubject(model.UserName, additionalClaims);
                return Json(new LoginResponseModel
                {
                    AccessToken = CreateToken(subject, this._identityOptions.AccessTokenExpires),
                    RefreshToken = CreateToken(subject, model.RememberMe ? TimeSpan.FromDays(365) : this._identityOptions.RefreshTokenExpires),
                    ExpiresIn = (long)this._identityOptions.AccessTokenExpires.TotalSeconds
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Problem(ex.ToString());
            }
        }
        return BadRequest(this.ModelState.ToErrors());
    }

    [HttpPost("[action]")]
    [AllowAnonymous]
    public LoginResponseModel Refresh(string refreshToken)
    {
        var validationResult = this._jwtSecurityTokenHandler.ValidateTokenAsync(refreshToken, this._tokenValidationParameters).Result;
        if (!validationResult.IsValid)
        {
            throw new Exception("RefreshToken验证失败", innerException: validationResult.Exception);
        }
        var subject = validationResult.ClaimsIdentity;
        return new LoginResponseModel
        {
            AccessToken = CreateToken(subject, this._identityOptions.AccessTokenExpires),
            RefreshToken = CreateToken(subject, validationResult.SecurityToken.ValidTo.Subtract(validationResult.SecurityToken.ValidFrom)),
            ExpiresIn = (long)this._identityOptions.AccessTokenExpires.TotalSeconds
        };
    }

    private ClaimsIdentity CreateSubject(string userName, List<Claim> additionalClaims)
    {
        var claims = new List<Claim>(additionalClaims) { new Claim(this._tokenValidationParameters.NameClaimType, userName) };
        var subject = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
        return subject;
    }

    private string CreateToken(ClaimsIdentity subject, TimeSpan timeout)
    {
        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            // 签发者
            Issuer = this._identityOptions.Issuer,
            // 接收方
            Audience = this._identityOptions.Audience,
            // 凭据
            SigningCredentials = _credentials,
            // 声明
            Subject = subject,
            // 签发时间
            IssuedAt = now,
            // 生效时间
            NotBefore = now,
            // UTC 过期时间
            Expires = now.Add(timeout),
        };
        var securityToken = this._jwtSecurityTokenHandler.CreateJwtSecurityToken(tokenDescriptor);
        var token = this._jwtSecurityTokenHandler.WriteToken(securityToken);
        return token;
    }
}
