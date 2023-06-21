using SkiaSharp;
using WTA.Shared.Attributes;

namespace WTA.Shared.Captcha;

[Implement<ICaptchaService>]
public class CaptchaService : ICaptchaService
{
    public string Create(string code)
    {
        using var image2d = new SKBitmap(120, 30, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(image2d);
        using var paint = new SKPaint() { TextSize = 20, TextAlign= SKTextAlign.Center };
        canvas.DrawColor(SKColors.White);
        canvas.DrawText(code, 15, 15, paint);
        using var image = SKImage.FromBitmap(image2d);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return $"data:image/png;base64,{Convert.ToBase64String(data.ToArray())}";
    }
}
