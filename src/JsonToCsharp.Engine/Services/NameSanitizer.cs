using System.Text.RegularExpressions;
using JsonToCsharp.Engine.Models;

namespace JsonToCsharp.Engine.Services;

/// <summary>
/// Converts raw JSON property names into valid C# identifiers,
/// applying the requested naming convention in the process.
///
/// Handles the common real-world cases that break naive generators:
/// hyphens, underscores, leading digits, special characters, dollar signs.
/// </summary>
internal static class NameSanitizer
{
    public static string SanitizePropertyName(string jsonName, NamingConvention convention)
    {
        if (string.IsNullOrWhiteSpace(jsonName))
            return "_unknown";

        var words = Regex.Split(jsonName, @"[-_\s\.]+")
                         .Where(w => w.Length > 0)
                         .ToArray();

        if (words.Length == 0)
            return "_unknown";

        var result = convention == NamingConvention.PascalCase
            ? string.Concat(words.Select(CapitalizeFirst))
            : char.ToLower(words[0][0]) + words[0][1..] +
              string.Concat(words.Skip(1).Select(CapitalizeFirst));

        result = Regex.Replace(result, @"[^\w]", "_");

        if (result.Length > 0 && char.IsDigit(result[0]))
            result = "_" + result;

        return string.IsNullOrEmpty(result) ? "_unknown" : result;
    }

    /// <summary>
    /// Class names are always PascalCase regardless of the property
    /// naming convention — this is a C# standard, not a user preference.
    /// </summary>
    public static string SanitizeClassName(string jsonName)
        => SanitizePropertyName(jsonName, NamingConvention.PascalCase);

    private static string CapitalizeFirst(string word)
        => word.Length == 0 ? word : char.ToUpper(word[0]) + word[1..].ToLower();
}