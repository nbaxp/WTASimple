using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;

namespace WTA.Shared.DataAnnotations;

public class CustomModelMetaDataProvider : DefaultModelMetadataProvider
{
    public CustomModelMetaDataProvider(ICompositeMetadataDetailsProvider detailsProvider, IOptions<MvcOptions> optionsAccessor) : base(detailsProvider, optionsAccessor)
    {
    }

    protected override ModelMetadata CreateModelMetadata(DefaultMetadataDetails entry)
    {
        return new CustomModelMetadata(this, DetailsProvider, entry, ModelBindingMessageProvider);
    }
}
