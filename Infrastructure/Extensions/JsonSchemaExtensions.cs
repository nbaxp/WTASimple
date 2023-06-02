namespace WTA.Infrastructure.Extensions;

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using WTA.Infrastructure.Application;

public static class JsonSchemaExtensions
{
    public static object GetMetadataForType(this Type modelType)
    {
        using var scope = WebApp.Current.Services.CreateScope();
        var meta = scope.ServiceProvider.GetRequiredService<IModelMetadataProvider>().GetMetadataForType(modelType);
        return meta.GetSchema(scope.ServiceProvider);
    }

    public static object GetSchema(this ModelMetadata meta, IServiceProvider serviceProvider, ModelMetadata? parent = null)
    {
        var schema = new Dictionary<string, object>();
        var modelType = meta.UnderlyingOrModelType;
        var title = meta.ContainerType == null ? modelType.GetDisplayName() : meta.ContainerType?.GetProperty(meta.PropertyName!)?.GetDisplayName() ?? meta.GetDisplayName();
        if (modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(PaginationModel<,>))
        {
            var genericType = modelType.GetGenericArguments()[0];
            title = genericType.GetDisplayName();
        }
        schema.Add("title", title);
        var showForDisplay = meta.ShowForDisplay;
        if (meta is DefaultModelMetadata defaultMeta)
        {
            var attribute = defaultMeta.Attributes.PropertyAttributes?.FirstOrDefault(o => o.GetType() == typeof(HiddenInputAttribute));
            if (attribute != null)
            {
                showForDisplay = (attribute as HiddenInputAttribute)!.DisplayValue;
            }
        }

        // array
        if (meta.IsEnumerableType)
        {
            if (modelType != meta.ElementMetadata!.ModelType.UnderlyingSystemType)
            {
                schema.Add("type", "array");
                //schema.Add("items", meta.ElementMetadata!.ModelType.GetMetadataForType(serviceProvider));
                schema.Add("items", meta.ElementMetadata.GetSchema(serviceProvider, meta));
            }
        }
        else
        {
            if (!modelType.IsValueType && modelType != typeof(string))
            {
                schema.Add("type", "object");
                var properties = new Dictionary<string, object>();
                foreach (var propertyMetadata in meta.Properties)
                {
                    if (meta.ContainerType != propertyMetadata.ContainerType)
                    {
                        if (propertyMetadata.IsEnumerableType)
                        {
                            //array
                            if (propertyMetadata.ElementType == propertyMetadata.ContainerType)
                            {
                                continue;
                            }
                        }
                        else if (!propertyMetadata.ModelType.IsValueType && propertyMetadata.ModelType != typeof(string))
                        {
                            //object
                            if (propertyMetadata.ModelType == propertyMetadata.ContainerType)
                            {
                                continue;
                            }
                            if (parent != null)
                            {
                                continue;
                            }
                        }
                        properties.Add(propertyMetadata.Name!, propertyMetadata.GetSchema(serviceProvider, meta));
                    }
                }
                schema.Add(nameof(properties), properties);
            }
            else
            {
                schema.Add("type", GetJsonType(modelType));
            }
        }

        schema.AddNotNull("description", meta.Description);
        schema.AddNotNull("format", meta.DataTypeName?.ToLowerCamelCase());
        schema.AddNotNull("control", meta.TemplateHint?.ToLowerCamelCase());
        //schema.AddNotNull(nameof(meta.ShowForDisplay), showForDisplay);
        //schema.AddNotNull(nameof(meta.ShowForEdit), meta.ShowForEdit);
        //schema.AddNotNull(nameof(meta.IsReadOnly), meta.IsReadOnly);

        var roles = meta.GetRules(serviceProvider, title);
        if (roles.Any())
        {
            schema.Add("rules", roles);
        }
        return schema;
    }

    public static List<Dictionary<string, object>> GetRules(this ModelMetadata meta, IServiceProvider serviceProvider, string title)
    {
        var pm = (meta as DefaultModelMetadata)!;
        var rules = new List<Dictionary<string, object>>();
        var validationProvider = serviceProvider.GetRequiredService<IValidationAttributeAdapterProvider>();
        var localizer = serviceProvider.GetRequiredService<IStringLocalizer>();
        var actionContext = new ActionContext { HttpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext! };
        var provider = new EmptyModelMetadataProvider();
        var modelValidationContextBase = new ModelValidationContextBase(actionContext, meta, new EmptyModelMetadataProvider());
        if (pm.IsRequired && !pm.IsNullableValueType && !pm.UnderlyingOrModelType.IsValueType && !pm.Attributes.Attributes.Any(o => o.GetType() == typeof(RequiredAttribute)))
        {
            var message = string.Format(CultureInfo.InvariantCulture, localizer.GetString(nameof(RequiredAttribute)).Value, title);
            rules.Add(new Dictionary<string, object> { { "required", true }, { "message", message } });
        }
        foreach (var item in pm.Attributes.Attributes)
        {
            if (item is ValidationAttribute attribute && !string.IsNullOrEmpty(attribute.ErrorMessage))
            {
                var errorMessage = localizer.GetString(attribute.ErrorMessage).Value;
                string? message;
                if (attribute is RemoteAttribute)
                {
                    message = string.Format(CultureInfo.InvariantCulture, errorMessage, title);
                }
                else if (attribute is DataTypeAttribute)
                {
                    if (attribute is FileExtensionsAttribute extensionsAttribute)
                    {
                        message = string.Format(CultureInfo.InvariantCulture, errorMessage, title, extensionsAttribute.Extensions);
                    }
                    else
                    {
                        message = string.Format(CultureInfo.InvariantCulture, errorMessage, title);
                    }
                }
                else
                {
                    message = validationProvider.GetAttributeAdapter(attribute!, localizer)?.GetErrorMessage(modelValidationContextBase);
                }
                var rule = new Dictionary<string, object>();
                if (attribute is RegularExpressionAttribute regularExpression)
                {
                    rule.Add("pattern", regularExpression.Pattern);
                }
                else if (attribute is MaxLengthAttribute maxLength)
                {
                    rule.Add("max", maxLength.Length);
                }
                else if (attribute is RequiredAttribute)
                {
                    rule.Add("required", true);
                    //message = string.Format(CultureInfo.InvariantCulture, localizer.GetString(nameof(RequiredAttribute)).Value, title);
                }
                else if (attribute is CompareAttribute compare)//??
                {
                    rule.Add("validator", "compare");
                    rule.Add("compare", compare.OtherProperty.ToLowerCamelCase());
                }
                else if (attribute is MinLengthAttribute minLength)
                {
                    rule.Add("min", minLength.Length);
                }
                else if (attribute is CreditCardAttribute)
                {
                    rule.Add("validator", "creditcard");
                }
                else if (attribute is StringLengthAttribute stringLength)
                {
                    rule.Add("min", stringLength.MinimumLength);
                    rule.Add("max", stringLength.MaximumLength);
                }
                else if (attribute is RangeAttribute range)
                {
                    rule.Add("type", "number");
                    rule.Add("min", range.Minimum is int minInt ? minInt : (double)range.Minimum);
                    rule.Add("max", range.Maximum is int maxInt ? maxInt : (double)range.Maximum);
                }
                else if (attribute is EmailAddressAttribute)
                {
                    rule.Add("type", "email");
                }
                else if (attribute is PhoneAttribute)
                {
                    rule.Add("validator", "phone");
                }
                else if (attribute is UrlAttribute)
                {
                    rule.Add("type", "url");
                }
                else if (attribute is FileExtensionsAttribute fileExtensions)
                {
                    rule.Add("validator", "accept");
                    rule.Add("extensions", fileExtensions.Extensions);
                }
                else if (attribute is RemoteAttribute remote)
                {
                    rule.Add("validator", "remote");
                    var attributes = new Dictionary<string, string>();
                    remote.AddValidation(new ClientModelValidationContext(actionContext, pm, provider, attributes));
                    rule.Add("remote", attributes["data-val-remote-url"]);
                    //rule.Add("fields", remote.AdditionalFields.Split(',').Where(o => !string.IsNullOrEmpty(o)).Select(o => o.ToLowerCamelCase()).ToList());
                }
                else if (attribute is DataTypeAttribute dataType)
                {
                    var name = dataType.GetDataTypeName();
                    if (name == DataType.DateTime.ToString())
                    {
                        rule.Add("type", "date");
                    }
                }
                else
                {
                    //Console.WriteLine($"{attribute.GetType().Name}");
                }
                rule.Add("message", message!);
                //rule.Add("trigger", "change");
                rules.Add(rule);
            }
            else
            {
                //Console.WriteLine($"{item.GetType().Name}");
            }
        }
        return rules;
    }

    private static string GetJsonType(Type modelType)
    {
        if (modelType == typeof(bool))
        {
            return "boolean";
        }
        else if (modelType == typeof(short) ||
            modelType == typeof(int) ||
            modelType == typeof(long))
        {
            return "integer";
        }
        else if (
            modelType == typeof(float) ||
            modelType == typeof(double) ||
            modelType == typeof(decimal))
        {
            return "number";
        }
        else if (modelType == typeof(string))
        {
            return "string";
        }
        else if (modelType.IsClass && !modelType.IsNullableType())
        {
            return "object";
        }
        return modelType.Name.ToLowerCamelCase();
    }
}
