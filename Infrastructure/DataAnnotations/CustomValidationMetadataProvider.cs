using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace WTA.Infrastructure.DataAnnotations;

public class CustomValidationMetadataProvider : IValidationMetadataProvider
{
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        if (context.Key.MetadataKind == ModelMetadataKind.Parameter || context.Key.MetadataKind == ModelMetadataKind.Property)
        {
            var nullabilityContext = new System.Reflection.NullabilityInfoContext();
            var nullability = context.Key.MetadataKind == ModelMetadataKind.Parameter ? nullabilityContext.Create(context.Key.ParameterInfo!) : nullabilityContext.Create(context.Key.PropertyInfo!);
            var isOptional = nullability != null && nullability.ReadState != System.Reflection.NullabilityState.NotNull;
            if (!isOptional)
            {
                var attribute = new RequiredAttribute { AllowEmptyStrings = false, ErrorMessage = nameof(RequiredAttribute) };
                context.ValidationMetadata.ValidatorMetadata.Add(attribute);
                context.ValidationMetadata.IsRequired = true;
            }
        }
    }
}
