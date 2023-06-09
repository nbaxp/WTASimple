using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WTA.Shared.Attributes;
using WTA.Shared.Data;

namespace WTA.Shared.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> Where<TEntity, TModel>(this IQueryable<TEntity> query, TModel model)
    {
        var properties = model!.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
            .Where(o => o.PropertyType.IsValueType || o.PropertyType == typeof(string))
            .Where(o => o.PropertyType != typeof(bool))
            .Where(o => !o.CustomAttributes.Any(o => o.AttributeType == typeof(ScaffoldColumnAttribute)));
        foreach (var property in properties)
        {
            var propertyName = property.Name;
            var propertyValue = property.GetValue(model, null);
            if (propertyValue != null)
            {
                var attributes = property.GetCustomAttributes<OperatorTypeAttribute>().Where(o => o.OperatorType != OperatorType.Ignore);
                if (attributes.Any())
                {
                    foreach (var attribute in attributes)
                    {
                        var targetPropertyName = attribute.PropertyName ?? propertyName;
                        if (typeof(TEntity).GetProperty(targetPropertyName) != null)
                        {
                            var expression = attribute.OperatorType.GetType().GetCustomAttribute<ExpressionAttribute>()?.Expression;
                            if (expression != null)
                            {
                                query = query.Where(string.Format(CultureInfo.InvariantCulture, expression, targetPropertyName), propertyValue);
                            }
                        }
                    }
                }
                else
                {
                    if (property.PropertyType == typeof(string))
                    {
                        if (typeof(TEntity).GetProperty(propertyName) != null)
                        {
                            var expression = OperatorType.Contains.GetAttributeOfType<ExpressionAttribute>()?.Expression!;
                            query = query.Where(string.Format(CultureInfo.InvariantCulture, expression, propertyName), propertyValue);
                        }
                    }
                    else if (property.PropertyType.GetUnderlyingType() == typeof(DateTime))
                    {
                        var start = $"{propertyName}Start";
                        if (typeof(TEntity).GetProperty(start) != null)
                        {
                            var expression = OperatorType.GreaterThanOrEqual.GetAttributeOfType<ExpressionAttribute>()?.Expression!;
                            query = query.Where(string.Format(CultureInfo.InvariantCulture, expression, start), propertyValue);
                        }
                        var end = $"{propertyName}End";
                        if (typeof(TEntity).GetProperty(end) != null)
                        {
                            var expression = OperatorType.LessThanOrEqual.GetAttributeOfType<ExpressionAttribute>()?.Expression!;
                            query = query.Where(string.Format(CultureInfo.InvariantCulture, expression, end), propertyValue);
                        }
                    }
                    else
                    {
                        if (typeof(TEntity).GetProperty(propertyName) != null)
                        {
                            var expression = OperatorType.Equal.GetAttributeOfType<ExpressionAttribute>()?.Expression!;
                            query = query.Where(string.Format(CultureInfo.InvariantCulture, expression, propertyName), propertyValue);
                        }
                    }
                }
            }
        }
        return query;
    }

    public static IQueryable<TEntity> Where<TEntity>(this IQueryable<TEntity> query, string where, params object?[] args)
    {
        return DynamicQueryableExtensions.Where(query, where, args);
    }

    public static List<TModel> Select<TEntity, TModel>(this IQueryable<TEntity> query, string select, params object?[] args)
    {
        return DynamicQueryableExtensions.Select(query, select, args).ToDynamicList<TModel>();
    }

    public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> query, string ordering, params object?[] args)
    {
        query = DynamicQueryableExtensions.OrderBy(query, ordering, args);
        return query;
    }

    public static IQueryable<TEntity> Include<TEntity>(this IQueryable<TEntity> query) where TEntity : class
    {
        var properties = typeof(TEntity).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
            .Where(o => o.Name.Length > 2 && o.Name.EndsWith("Id"))
            .Where(o => o.PropertyType.IsValueType && o.PropertyType.GetUnderlyingType() == typeof(Guid));
        foreach (var property in properties)
        {
            var propertyName = property.Name.Substring(0, property.Name.Length - 2);
            if (typeof(TEntity).GetProperty(propertyName) != null)
            {
                query = EntityFrameworkQueryableExtensions.Include(query, propertyName);
            }
        }
        return query;
    }
}
