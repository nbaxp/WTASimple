using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace WTA.Shared.Options;

public class EmbeddedConfigureOptions : IPostConfigureOptions<StaticFileOptions>
{
    public void PostConfigure(string? name, StaticFileOptions options)
    {
        var providers = new List<IFileProvider>
        {
            //new ManifestEmbeddedFileProvider(typeof(EmbeddedConfigureOptions).Assembly, "wwwroot"),
        };
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        if (Directory.Exists(path))
        {
            providers.Add(new PhysicalFileProvider(path));
        }
        options.FileProvider = new CompositeFileProvider(providers.ToArray());
        var provider = new FileExtensionContentTypeProvider();
        options.ContentTypeProvider = provider;
        options.ServeUnknownFileTypes = true;
    }
}
