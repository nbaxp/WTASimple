﻿using WTA.Infrastructure.Data;

namespace WTA.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class OperatorTypeAttribute : Attribute
{
    public OperatorTypeAttribute(OperatorType operatorType, string? propertyName = null)
    {
        this.OperatorType = operatorType;
        this.PropertyName = propertyName;
    }

    public OperatorType OperatorType { get; }
    public string? PropertyName { get; }
}