using Newtonsoft.Json.Linq;
using JsonToCsharp.Engine.Models;

namespace JsonToCsharp.Engine.Services;

/// <summary>
/// Resolves a JToken to its corresponding C# type name.
/// Behaviour changes based on whether UsePreciseTypes is enabled in options.
/// </summary>
internal static class TypeResolver
{
    public static string Resolve(JToken token, GenerationOptions options)
    {
        return token.Type switch
        {
            JTokenType.String => ResolveString(token, options),
            JTokenType.Boolean => "bool",
            JTokenType.Integer => ResolveInteger(token, options),
            JTokenType.Float => ResolveFloat(token, options),
            JTokenType.Null => options.UseNullableReferenceTypes ? "object?" : "object",

            // Newtonsoft automatically parses ISO 8601 date strings into JTokenType.Date
            JTokenType.Date => options.UsePreciseTypes ? "DateTime" :
                                  (options.UseNullableReferenceTypes ? "string?" : "string"),

            // Object and Array are handled by the recursive walker in the
            // converter — TypeResolver only handles leaf/primitive values.
            JTokenType.Object => "object",
            JTokenType.Array => "object[]",

            _ => "object"
        };
    }

    // -------------------------------------------------------------------------
    // String resolution
    // -------------------------------------------------------------------------

    private static string ResolveString(JToken token, GenerationOptions options)
    {
        var nullable = options.UseNullableReferenceTypes ? "?" : "";

        // In simple mode we always return string — fast, honest, no surprises.
        if (!options.UsePreciseTypes)
            return $"string{nullable}";

        // In precise mode we inspect the value and try to recognise
        // common patterns that map to more specific .NET types.
        var value = token.Value<string>() ?? string.Empty;

        if (DateTime.TryParse(value, out _))
            return "DateTime";

        if (Guid.TryParse(value, out _))
            return "Guid";

        if (Uri.TryCreate(value, UriKind.Absolute, out _))
            return $"Uri{nullable}";

        return $"string{nullable}";
    }

    // -------------------------------------------------------------------------
    // Number resolution
    // -------------------------------------------------------------------------

    private static string ResolveInteger(JToken token, GenerationOptions options)
    {
        // Simple mode: always int. The developer knows if they need long.
        if (!options.UsePreciseTypes)
            return "int";

        // Precise mode: fit the value into the smallest correct type.
        var value = token.Value<long>();
        return value is >= int.MinValue and <= int.MaxValue ? "int" : "long";
    }

    private static string ResolveFloat(JToken token, GenerationOptions options)
    {
        // Simple mode: always double — decimal is rarely what you want
        if (!options.UsePreciseTypes)
            return "double";

        // Precise mode: check if the value needs decimal precision.
        var raw = token.ToString();
        if (decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d))
        {
            if (d != (decimal)(double)d)
                return "decimal";
        }

        return "double";
    }
}