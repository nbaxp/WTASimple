namespace WTA.Shared.Extensions;

public static class DictionaryExtensions
{
    public static void AddNotNull<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue? value) where TKey : notnull
    {
        if (!dictionary.ContainsKey(key) && value != null)
        {
            dictionary.Add(key, value);
        }
    }
}
