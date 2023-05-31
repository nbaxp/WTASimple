namespace WTA.Infrastructure.Extensions;

public static class EnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
    {
        values.ToList().ForEach(action);
    }
}