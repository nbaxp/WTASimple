using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace WTA.Shared.DataAnnotations;

public class CustomDisplayMetadataProvider : IDisplayMetadataProvider
{
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        var attributes = context.Attributes;
        var displayAttribute = attributes.OfType<DisplayAttribute>().FirstOrDefault();
        if (displayAttribute != null && string.IsNullOrEmpty(displayAttribute.Name))
        {
            displayAttribute.Name = $"{context.Key.ContainerType?.Name}.{context.Key.Name}";
        }
        //此处必须保留
        foreach (var item in attributes)
        {
            if (item is ValidationAttribute attribute)
            {
                if (attribute is DataTypeAttribute data && attribute.ErrorMessage != null)
                {
                    attribute.ErrorMessage = $"DataTypeAttribute_{data.GetDataTypeName()}";
                }
                else if (item is RequiredAttribute required)
                {
                    required.ErrorMessage = nameof(RequiredAttribute);
                }
                else
                {
                    if (attribute.ErrorMessage == null)
                    {
                        attribute.ErrorMessage = attribute.GetType().Name;
                        if (item is StringLengthAttribute stringLengthAttribute)
                        {
                            if (stringLengthAttribute.MinimumLength != 0)
                            {
                                attribute.ErrorMessage += "IncludingMinimum";
                            }
                        }
                    }
                }
            }
        }
    }
}
