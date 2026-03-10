using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonToCsharp.Engine.Interfaces;
using JsonToCsharp.Engine.Models;

namespace JsonToCsharp.Engine.Services;

public class JsonToCsharpConverter : IJsonToCsharpConverter
{
    public ConversionResult Convert(string json, GenerationOptions options)
    {
        if (string.IsNullOrWhiteSpace(json))
            return ConversionResult.Fail("Input JSON cannot be empty.");

        try
        {
            var token = JToken.Parse(json);

            if (token.Type != JTokenType.Object)
                return ConversionResult.Fail(
                    "Root element must be a JSON object. " +
                    "Array roots will be supported in a future update.");

            var output = GenerateClass((JObject)token, options.RootClassName, options);
            return ConversionResult.Ok(output);
        }
        catch (JsonReaderException ex)
        {
            return ConversionResult.Fail($"Invalid JSON: {ex.Message}");
        }
    }

    private string GenerateClass(JObject obj, string className, GenerationOptions options)
    {
        var sb = new StringBuilder();
        var sanitizedClassName = NameSanitizer.SanitizeClassName(className);
        var keyword = options.GenerateAsRecord ? "record" : "class";

        sb.AppendLine($"public {keyword} {sanitizedClassName}");
        sb.AppendLine("{");

        foreach (var property in obj.Properties())
        {
            if (property.Value.Type == JTokenType.Object)
            {
                sb.AppendLine($"    // TODO: nested object '{property.Name}' — not yet supported");
                continue;
            }

            if (property.Value.Type == JTokenType.Array)
            {
                sb.AppendLine($"    // TODO: array '{property.Name}' — not yet supported");
                continue;
            }

            var csharpType = TypeResolver.Resolve(property.Value, options);
            var propertyName = NameSanitizer.SanitizePropertyName(
                property.Name, options.NamingConvention);

            var nameWasChanged = propertyName != property.Name;
            var attributeRequested = options.AttributeStyle != AttributeStyle.None;

            if (nameWasChanged || attributeRequested)
                sb.AppendLine(BuildAttribute(property.Name, options.AttributeStyle));

            if (csharpType is "object?" or "object")
                sb.AppendLine($"    // TODO: replace 'object' with the actual type");

            sb.AppendLine($"    public {csharpType} {propertyName} {{ get; set; }}");
            sb.AppendLine();
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildAttribute(string jsonName, AttributeStyle style)
    {
        return style switch
        {
            AttributeStyle.Newtonsoft =>
                $"    [Newtonsoft.Json.JsonProperty(\"{jsonName}\")]",
            _ =>
                $"    [System.Text.Json.Serialization.JsonPropertyName(\"{jsonName}\")]"
        };
    }
}