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
            var collector = new ClassCollector();

            switch (token.Type)
            {
                case JTokenType.Object:
                    GenerateClass((JObject)token, options.RootClassName, options, collector);
                    return ConversionResult.Ok(PrependUsings(collector.BuildOutput(), options));

                case JTokenType.Array:
                    var array = (JArray)token;
                    return HandleRootArray(array, options, collector);

                default:
                    return ConversionResult.Fail(
                        "Root element must be a JSON object or array.");
            }
        }
        catch (JsonReaderException ex)
        {
            return ConversionResult.Fail($"Invalid JSON: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------
    // Root array handling
    // -------------------------------------------------------------------------

    private ConversionResult HandleRootArray(
        JArray array,
        GenerationOptions options,
        ClassCollector collector)
    {
        if (array.Count == 0)
            return ConversionResult.Fail(
                "Cannot infer a schema from an empty array.");

        // Check whether the array contains objects or primitives.
        var firstNonNull = array.FirstOrDefault(t => t.Type != JTokenType.Null);

        if (firstNonNull == null)
            return ConversionResult.Fail(
                "Cannot infer a schema — all array elements are null.");

        if (firstNonNull.Type != JTokenType.Object)
        {
            var primitiveType = TypeResolver.Resolve(firstNonNull, options);
            return ConversionResult.Ok(
                $"// Root type: List<{primitiveType}>\n" +
                $"// No class generated — element type is a primitive.");
        }

        var schema = ObjectSchemaInferrer.Infer(array, options);
        var className = GenerateClassFromSchema(
            schema, options.ArrayItemClassName, options, collector);

        var output = collector.BuildOutput() +
                     Environment.NewLine +
                     $"// Root type: List<{className}>";

        return ConversionResult.Ok(PrependUsings(output, options));
    }

    // -------------------------------------------------------------------------
    // Class generation from a raw JObject (nested objects, root object)
    // -------------------------------------------------------------------------

    private string GenerateClass(
        JObject obj,
        string className,
        GenerationOptions options,
        ClassCollector collector)
    {
        // Build the inferred schema from a single-element array so we can
        // reuse GenerateClassFromSchema for both paths.
        var singleItemArray = new JArray(obj);
        var schema = ObjectSchemaInferrer.Infer(singleItemArray, options);
        return GenerateClassFromSchema(schema, className, options, collector);
    }

    // -------------------------------------------------------------------------
    // Core class generation — shared by both single-object and array paths
    // -------------------------------------------------------------------------

    private string GenerateClassFromSchema(
        Dictionary<string, ObjectSchemaInferrer.InferredProperty> schema,
        string className,
        GenerationOptions options,
        ClassCollector collector)
    {
        var sb = new StringBuilder();
        var sanitizedClassName = NameSanitizer.SanitizeClassName(className);
        var keyword = options.GenerateAsRecord ? "record" : "class";

        sb.AppendLine($"public {keyword} {sanitizedClassName}");
        sb.AppendLine("{");

        foreach (var (jsonName, inferredProp) in schema)
        {
            var propertyName = NameSanitizer.SanitizePropertyName(
                jsonName, options.NamingConvention);

            // Determine the C# type — handling depends on what kind of
            // value this property holds across its observed instances.
            string csharpType;
            string? comment = null;

            var sample = inferredProp.NonNullSamples.FirstOrDefault();

            if (sample == null)
            {
                csharpType = options.UseNullableReferenceTypes ? "object?" : "object";
                comment = "// TODO: replace 'object' with the actual type — only null values observed";
            }
            else if (sample.Type == JTokenType.Object)
            {
                csharpType = ResolveNestedObject(
                    inferredProp, jsonName, options, collector);

                if (inferredProp.IsNullableOrMissing && !csharpType.EndsWith('?'))
                    csharpType += "?";
            }
            else if (sample.Type == JTokenType.Array)
            {
                csharpType = ResolveArray(
                    (JArray)sample, jsonName, options, collector);
            }
            else
            {
                (csharpType, comment) = ObjectSchemaInferrer.ResolveType(
                    inferredProp, options);
            }

            var nameWasChanged = !string.Equals(propertyName, jsonName, StringComparison.OrdinalIgnoreCase);
            var attributeRequested = options.AttributeStyle != AttributeStyle.None;

            if (nameWasChanged || attributeRequested)
                sb.AppendLine(BuildAttribute(jsonName, options.AttributeStyle));

            if (comment != null)
                sb.AppendLine($"    {comment}");

            var accessor = options.GenerateAsRecord ? "get; init;" : "get; set;";
            sb.AppendLine($"    public {csharpType} {propertyName} {{ {accessor} }}");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        var classBody = sb.ToString();
        var registeredName = collector.Register(sanitizedClassName, classBody);

        // If the collector assigned a different name due to collision, update the
        // class body so the declaration matches the registered name.
        if (registeredName != sanitizedClassName)
        {
            classBody = classBody.Replace(
                $"{keyword} {sanitizedClassName}",
                $"{keyword} {registeredName}");
            // Re-register with the corrected body
            collector.UpdateBody(registeredName, classBody);
        }

        return registeredName;
    }

    // -------------------------------------------------------------------------
    // Nested object resolution
    // -------------------------------------------------------------------------

    private string ResolveNestedObject(
        ObjectSchemaInferrer.InferredProperty prop,
        string jsonPropertyName,
        GenerationOptions options,
        ClassCollector collector)
    {
        // Collect all non-null object samples and merge their schemas.
        var nestedArray = new JArray(prop.NonNullSamples);
        var nestedSchema = ObjectSchemaInferrer.Infer(nestedArray, options);

        // Derive the nested class name from the property name.
        // "address" → "Address", "homeAddress" → "HomeAddress"
        var nestedClassName = NameSanitizer.SanitizeClassName(jsonPropertyName);

        return GenerateClassFromSchema(nestedSchema, nestedClassName, options, collector);
    }

    // -------------------------------------------------------------------------
    // Array property resolution
    // -------------------------------------------------------------------------

    private string ResolveArray(
        JArray array,
        string jsonPropertyName,
        GenerationOptions options,
        ClassCollector collector)
    {
        if (array.Count == 0)
            return "List<object> // TODO: empty array — specify element type";

        var firstNonNull = array.FirstOrDefault(t => t.Type != JTokenType.Null);

        if (firstNonNull == null)
            return options.UseNullableReferenceTypes
                ? "List<object?> // TODO: all array elements were null"
                : "List<object> // TODO: all array elements were null";

        if (firstNonNull.Type == JTokenType.Object)
        {
            var nestedSchema = ObjectSchemaInferrer.Infer(array, options);
            var singularName = jsonPropertyName;
            if (singularName.EndsWith("es", StringComparison.OrdinalIgnoreCase) && singularName.Length > 3)
                singularName = singularName[..^2];  // "addresses" → "address"
            else if (singularName.EndsWith('s') && singularName.Length > 2)
                singularName = singularName[..^1];  // "tags" → "tag"

            var elementClassName = NameSanitizer.SanitizeClassName(singularName);

            var registeredName = GenerateClassFromSchema(
                nestedSchema, elementClassName, options, collector);

            return $"List<{registeredName}>";
        }

        // Array of primitives — check for type consistency across all elements.
        var types = array
            .Where(t => t.Type != JTokenType.Null)
            .Select(t => t.Type)
            .Distinct()
            .ToList();

        if (types.Count > 1)
            return "List<object> // TODO: mixed element types — verify manually";

        var elementType = TypeResolver.Resolve(firstNonNull, options);

        // Strip nullable marker from list element types
        elementType = elementType.TrimEnd('?');

        return $"List<{elementType}>";
    }

    // -------------------------------------------------------------------------
    // Attribute generation
    // -------------------------------------------------------------------------

    private static string BuildAttribute(string jsonName, AttributeStyle style)
    {
        return style switch
        {
            AttributeStyle.Newtonsoft =>
                $"    [JsonProperty(\"{jsonName}\")]",
            _ =>
                $"    [JsonPropertyName(\"{jsonName}\")]"
        };
    }

    private static string PrependUsings(string output, GenerationOptions options)
    {
        return options.AttributeStyle switch
        {
            AttributeStyle.SystemTextJson =>
                $"using System.Text.Json.Serialization;{Environment.NewLine}{Environment.NewLine}{output}",
            AttributeStyle.Newtonsoft =>
                $"using Newtonsoft.Json;{Environment.NewLine}{Environment.NewLine}{output}",
            // AttributeStyle.None — no using needed, return output unchanged
            _ => output
        };
    }
}