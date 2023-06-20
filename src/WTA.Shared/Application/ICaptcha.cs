using System.ComponentModel.DataAnnotations;

namespace WTA.Shared.Application;

public interface ICaptcha : IValidatableObject
{
    string Captcha { get; set; }
    string CaptchaState { get; set; }

    IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Captcha))
        {
            yield return new ValidationResult("验证码不能为空", new string[] { nameof(Captcha) });
        }
    }
}
