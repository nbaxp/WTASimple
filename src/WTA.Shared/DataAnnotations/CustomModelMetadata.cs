using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using WTA.Shared.Extensions;

namespace WTA.Shared.DataAnnotations;

public class CustomModelMetadata : DefaultModelMetadata
{
    public CustomModelMetadata(IModelMetadataProvider provider, ICompositeMetadataDetailsProvider detailsProvider, DefaultMetadataDetails details, DefaultModelBindingMessageProvider modelBindingMessageProvider) : base(provider, detailsProvider, details, modelBindingMessageProvider)
    {
    }

    public override string? DisplayName => this.ContainerType == null ? this.ModelType.GetDisplayName() : this.ContainerType?.GetProperty(this.PropertyName!)?.GetDisplayName() ?? this.GetDisplayName();
}
