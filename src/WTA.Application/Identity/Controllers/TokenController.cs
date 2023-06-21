using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WTA.Application.Identity.Entities.SystemManagement;
using WTA.Application.Identity.Entities.Tenants;
using WTA.Application.Identity.Models;
using WTA.Shared.Application;
using WTA.Shared.Authentication;
using WTA.Shared.Controllers;
using WTA.Shared.Data;
using WTA.Shared.Extensions;
using WTA.Shared.Identity;

namespace WTA.Application.Identity.Controllers;

[Route("api/{culture=zh}/[controller]")]
[ApiExplorerSettings(GroupName = nameof(IdentityModule))]
public class TokenController : BaseController, IResourceService<Token>
{
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly IdentityOptions _identityOptions;
    private readonly IPasswordHasher _passwordHasher;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly SigningCredentials _credentials;
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IRepository<User> _userRepository;

    public TokenController(TokenValidationParameters tokenValidationParameters,
        JwtSecurityTokenHandler jwtSecurityTokenHandler,
        SigningCredentials credentials,
        IOptions<IdentityOptions> identityOptions,
        IPasswordHasher passwordHasher,
        IRepository<Tenant> _tenantRepository,
        IRepository<User> userRepository)
    {
        this._tokenValidationParameters = tokenValidationParameters;
        this._jwtSecurityTokenHandler = jwtSecurityTokenHandler;
        this._credentials = credentials;
        this._identityOptions = identityOptions.Value;
        this._passwordHasher = passwordHasher;
        this._tenantRepository = _tenantRepository;
        this._userRepository = userRepository;
        this._userRepository.DisableTenantFilter();
    }

    [HttpGet("[action]")]
    [AllowAnonymous]
    public object Create()
    {
        return typeof(LoginRequestModel).GetViewModel();
    }

    [HttpPost("[action]")]
    [AllowAnonymous]
    public IActionResult Create([FromBody] LoginRequestModel model)
    {
        if (this.ModelState.IsValid)
        {
            try
            {
                var additionalClaims = new List<Claim>();
                if (model.TenantId != null)
                {
                    if (this._tenantRepository.AsNoTracking().Any(o => o.Id == model.TenantId))
                    {
                        additionalClaims.Add(new Claim(nameof(model.TenantId), model.TenantId?.ToString()!));
                    }
                    else
                    {//租户不存在
                        this.ModelState.AddModelError(nameof(LoginRequestModel.TenantId), "租户不存在");
                    }
                }
                if (this.ModelState.IsValid)
                {
                    var userQuery = this._userRepository.Queryable();
                    var user = userQuery.FirstOrDefault(o => o.UserName == model.UserName);
                    if (!this._identityOptions.SupportsUserLockout)
                    {//未启用登录锁定
                        if (user == null || user.PasswordHash != this._passwordHasher.HashPassword(model.Password, user.SecurityStamp!))
                        {
                            this.ModelState.AddModelError("", "用户名或密码错误");
                        }
                    }
                    else
                    {//启用登录锁定
                        if (user != null)
                        {
                            if (user.LockoutEnd.HasValue)
                            {//已锁定
                                if (DateTime.UtcNow > user.LockoutEnd.Value)
                                {//超时则解锁
                                    user.AccessFailedCount = 0;
                                    user.LockoutEnd = null;
                                    this._userRepository.SaveChanges();
                                }
                                else
                                {//未超时禁止登录
                                    var minutes = (user.LockoutEnd!.Value - DateTime.UtcNow).TotalMinutes.ToString("f0", CultureInfo.InvariantCulture);
                                    this.ModelState.AddModelError(nameof(LoginRequestModel.UserName), $"用户已锁定,{minutes} 分钟后解除锁定");
                                }
                            }
                            if (!user.LockoutEnd.HasValue)
                            {//未锁定
                                if (user.PasswordHash != this._passwordHasher.HashPassword(model.Password, user.SecurityStamp!))
                                {//密码不正确
                                    user.AccessFailedCount++;
                                    if (user.AccessFailedCount >= this._identityOptions.MaxFailedAccessAttempts)
                                    {//超过次数则锁定
                                        user.LockoutEnd = DateTime.UtcNow + this._identityOptions.DefaultLockoutTimeSpan;
                                        var minutes = (user.LockoutEnd!.Value - DateTime.UtcNow).TotalMinutes.ToString("f0", CultureInfo.InvariantCulture);
                                        this.ModelState.AddModelError(nameof(LoginRequestModel.UserName), $"用户已锁定,{minutes} 分钟后解除锁定");
                                    }
                                    else
                                    {//未超过次数提示剩余次数
                                        this.ModelState.AddModelError(nameof(LoginRequestModel.Password), $"密码错误,{this._identityOptions.MaxFailedAccessAttempts - user.AccessFailedCount}次失败后将锁定用户");
                                    }
                                    this._userRepository.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            this.ModelState.AddModelError(nameof(LoginRequestModel.UserName), "用户不存在");
                        }
                    }
                    if (this.ModelState.IsValid)
                    {//验证成功
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
                }
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
