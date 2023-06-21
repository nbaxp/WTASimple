using System.ComponentModel.DataAnnotations;

namespace WTA.Application.Identity.Models;

public class LoginRequestModel// : IValidatableObject
{
    [UIHint("select")]
    public Guid? TenantId { get; set; } = null!;

    [MaxLength(64)]
    public string UserName { get; set; } = null!;

    [MaxLength(64)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    //[ScaffoldColumn(false)]
    //public string CaptchaKey { get; set; } = null!;

    //[UIHint("captcha")]
    //public string Captcha { get; set; } = null!;

    public bool RememberMe { get; set; }

    //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    //{
    //    using var scope = WebApp.Current.Services.CreateScope();
    //    var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
    //    var code = cache.GetString(this.CaptchaKey);
    //    if (code == null)
    //    {
    //        yield return new ValidationResult("CaptchaExpired", new string[] { "Captcha" });
    //    }
    //    else if (code != this.Captcha)
    //    {
    //        yield return new ValidationResult("CaptchaError", new string[] { "Captcha" });
    //    }
    //}
}
