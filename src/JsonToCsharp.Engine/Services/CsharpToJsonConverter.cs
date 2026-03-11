using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using JsonToCsharp.Engine.Interfaces;
using JsonToCsharp.Engine.Models;

namespace JsonToCsharp.Engine.Services;

public class CsharpToJsonConverter : ICsharpToJsonConverter
{
    private static readonly Regex PropertyRegex = new(
        @"public\s+([\w\<\>\[\]\?,\s]+?)\s+(\w+)\s*\{\s*get;",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex ClassRegex = new(
        @"(?:public\s+)?(?:class|record)\s+(\w+)[^{]*\{(.*?)\}(?:\s*$|\s*(?:public|internal|private))",
        RegexOptions.Compiled | RegexOptions.Singleline);

    public ConversionResult Convert(string csharpCode)
    {
        if (string.IsNullOrWhiteSpace(csharpCode))
            return ConversionResult.Fail("Input C# code cannot be empty.");

        try
        {
            var classes = ExtractClasses(csharpCode);

            if (classes.Count == 0)
                return ConversionResult.Fail(
                    "No class or record definition found in the input. " +
                    "Make sure your input contains at least one public class or record.");

            var rootBody = classes.First().Value;
            var json = GenerateJson(rootBody, classes, new HashSet<string>(
                StringComparer.OrdinalIgnoreCase));

            var formatted = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<JsonElement>(json),
                new JsonSerializerOptions { WriteIndented = true });

            return ConversionResult.Ok(formatted);
        }
        catch (Exception ex)
        {
            return ConversionResult.Fail($"Failed to parse C# code: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------
    // Class extraction
    // -------------------------------------------------------------------------

    private static Dictionary<string, string> ExtractClasses(string code)
    {
        var classes = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        // Find all class/record declarations by their opening keyword
        var headerRegex = new Regex(
            @"(?:public\s+)?(?:class|record)\s+(\w+)[^{]*\{",
            RegexOptions.Compiled);

        foreach (Match match in headerRegex.Matches(code))
        {
            var className = match.Groups[1].Value.Trim();

            // Start counting braces from the opening brace of the class body.
            // We need to find the matching closing brace, not just the first one.
            var startIndex = match.Index + match.Length - 1; // position of '{'
            var depth = 0;
            var bodyStart = startIndex + 1;
            var bodyEnd = bodyStart;

            for (var i = startIndex; i < code.Length; i++)
            {
                if (code[i] == '{') depth++;
                else if (code[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        bodyEnd = i;
                        break;
                    }
                }
            }

            var classBody = code[bodyStart..bodyEnd].Trim();
            if (!string.IsNullOrWhiteSpace(className))
                classes[className] = classBody;
        }

        return classes;
    }
    // -------------------------------------------------------------------------
    // JSON generation
    // -------------------------------------------------------------------------

    private string GenerateJson(
        string classBody,
        Dictionary<string, string> allClasses,
        HashSet<string> visitedTypes)
    {
        var sb = new StringBuilder();
        sb.Append('{');

        var properties = PropertyRegex.Matches(classBody);
        var first = true;

        foreach (Match match in properties)
        {
            var typeName = match.Groups[1].Value.Trim();
            var propName = match.Groups[2].Value.Trim();

            if (propName is "get" or "set" or "init" or "value")
                continue;

            if (!first) sb.Append(',');
            first = false;

            // JSON convention is camelCase keys
            var jsonKey = char.ToLower(propName[0]) + propName[1..];
            sb.Append($"\"{jsonKey}\":");
            sb.Append(ResolveValue(typeName, allClasses, visitedTypes));
        }

        sb.Append('}');
        return sb.ToString();
    }

    // -------------------------------------------------------------------------
    // Type resolution
    // -------------------------------------------------------------------------

    private string ResolveValue(
        string typeName,
        Dictionary<string, string> allClasses,
        HashSet<string> visitedTypes)
    {
        // Strip nullable marker before matching
        var baseType = typeName.TrimEnd('?').Trim();

        return baseType.ToLowerInvariant() switch
        {
            "string" => "\"string\"",
            "int" => "0",
            "long" => "0",
            "short" => "0",
            "byte" => "0",
            "double" => "0.0",
            "decimal" => "0.0",
            "float" => "0.0",
            "bool" => "false",
            "datetime" => "\"2024-01-01T00:00:00Z\"",
            "guid" => "\"00000000-0000-0000-0000-000000000000\"",
            "uri" => "\"https://example.com\"",
            "object" => "null",
            _ => ResolveComplexType(baseType, allClasses, visitedTypes)
        };
    }

    private string ResolveComplexType(
        string typeName,
        Dictionary<string, string> allClasses,
        HashSet<string> visitedTypes)
    {
        // List<T>, IEnumerable<T>, ICollection<T>, IList<T>
        var listMatch = Regex.Match(typeName,
            @"^(?:List|IEnumerable|ICollection|IList|IReadOnlyList)<(.+)>$",
            RegexOptions.IgnoreCase);

        if (listMatch.Success)
        {
            var elementType = listMatch.Groups[1].Value.Trim();
            var elementValue = ResolveValue(elementType, allClasses, visitedTypes);
            return $"[{elementValue}]";
        }

        // T[] array syntax
        if (typeName.EndsWith("[]"))
        {
            var elementType = typeName[..^2].Trim();
            var elementValue = ResolveValue(elementType, allClasses, visitedTypes);
            return $"[{elementValue}]";
        }

        // Known class reference — recurse into nested object.
        // Guard against circular references.
        if (allClasses.TryGetValue(typeName, out var classBody))
        {
            if (visitedTypes.Contains(typeName))
                return "null";

            visitedTypes.Add(typeName);
            var nested = GenerateJson(classBody, allClasses, visitedTypes);
            visitedTypes.Remove(typeName);
            return nested;
        }

        // Unknown type — emit empty object placeholder
        return "{}";
    }
}