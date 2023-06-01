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

    public static string ToUnderline(this string input)
    {
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        return Regex.Replace(input.ToString()!, "([a-z])([A-Z])", "$1_$2").ToLowerInvariant();
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

    public static string ToLowerCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input) || !char.IsUpper(input[0]))
        {
            return input;
        }
        var chars = input.ToCharArray();
        FixCasing(chars);
        return new string(chars);
    }

    private static void FixCasing(Span<char> chars)
    {
        for (var i = 0; i < chars.Length; i++)
        {
            if (i == 1 && !char.IsUpper(chars[i]))
            {
                break;
            }

            var hasNext = i + 1 < chars.Length;

            // Stop when next char is already lowercase.
            if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
            {
                // If the next char is a space, lowercase current char before exiting.
                if (chars[i + 1] == ' ')
                {
                    chars[i] = char.ToLowerInvariant(chars[i]);
                }

                break;
            }

            chars[i] = char.ToLowerInvariant(chars[i]);
        }
    }
}
