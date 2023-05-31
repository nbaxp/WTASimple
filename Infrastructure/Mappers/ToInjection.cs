using Omu.ValueInjecter.Injections;
using System.Collections;
using System.Reflection;
using WTA.Infrastructure.Extensions;

namespace WTA.Infrastructure.Mappers;

public class ToInjection : LoopInjection
{
    protected override bool MatchTypes(Type source, Type target)
    {
        source = source.GetUnderlyingType();
        target = target.GetUnderlyingType();
        if (source != typeof(string) &&
            target != typeof(string) &&
            source.IsGenericType &&
            target.IsGenericType &&
            source.IsAssignableTo(typeof(IEnumerable)) &&
            source.IsAssignableTo(typeof(IEnumerable))
            )
        {
            return true;
        }
        return base.MatchTypes(source, target);
    }

    protected override void SetValue(object source, object target, PropertyInfo sp, PropertyInfo tp)
    {
        if (sp.PropertyType != typeof(string) && sp.PropertyType != typeof(string))
        {
            if (sp.PropertyType.IsAssignableTo(typeof(IDictionary)) && tp.PropertyType.IsAssignableTo(typeof(IDictionary)))
            {
                var targetKeyType = tp.PropertyType.GetGenericArguments()[0];
                var targetValueType = tp.PropertyType.GetGenericArguments()[1];
                var targetType = typeof(Dictionary<,>).MakeGenericType(targetKeyType, targetValueType);
                var addMethod = targetType.GetMethod("Add");
                var list = Activator.CreateInstance(targetType);
                var sourceList = (IDictionary)sp.GetValue(source)!;
                foreach (var item in sourceList)
                {
                    var key = typeof(DictionaryEntry).GetProperty("Key")?.GetValue(item)!;
                    var value = typeof(DictionaryEntry).GetProperty("Value")?.GetValue(item)!;
                    var key2 = targetKeyType == typeof(string) ? key.ToString() : Activator.CreateInstance(targetKeyType)?.FromObject(key)!;
                    var value2 = targetValueType == typeof(string) ? value.ToString() : Activator.CreateInstance(targetValueType)?.FromObject(value)!;
                    addMethod?.Invoke(list, new[] { key2, value2 });
                }
                tp.SetValue(target, list);
                return;
            }
            else if (sp.PropertyType.IsAssignableTo(typeof(IList)) && tp.PropertyType.IsAssignableTo(typeof(IList)))
            {
                var targetGenericType = tp.PropertyType.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(targetGenericType);
                var addMethod = listType.GetMethod("Add");
                var list = Activator.CreateInstance(listType);
                var sourceList = (ICollection)sp.GetValue(source)!;
                foreach (var item in sourceList)
                {
                    addMethod?.Invoke(list, new[] { targetGenericType == typeof(string) ? item.ToString() : Activator.CreateInstance(targetGenericType).FromObject(item) });
                }
                tp.SetValue(target, list);
                return;
            }
        }
        base.SetValue(source, target, sp, tp);
    }
}