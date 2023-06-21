namespace WTA.Shared.Captcha;

public interface ICaptchaService
{
    string Create(string code);
}
