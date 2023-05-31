using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WTA.Infrastructure.Extensions;

public static class StringExtensions
{
    public static string ToMd5(this string input)
    {
        return BitConverter.ToString(MD5.HashData(Encoding.ASCII.GetBytes(input))).Replace("-", "");
    }

    public static Guid ToGuid(this string input)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }

    public static string ToSlugify(this string input)
    {
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        return Regex.Replace(input!, "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
    }

    public static string TrimStart(this string input, string start)
    {
        return input.StartsWith(start) ? input[start.Length..] : input;
    }

    public static string TrimEnd(this string input, string end)
    {
        return input.EndsWith(end) ? input[..^end.Length] : input;
    }
}