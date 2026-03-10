using Newtonsoft.Json.Linq;
using JsonToCsharp.Engine.Models;

namespace JsonToCsharp.Engine.Services;

/// <summary>
/// Infers a merged property schema from an array of JSON objects.
/// Rather than looking at just the first instance, it walks every object
/// in the array and accumulates type evidence for each property.
/// The more instances, the more accurate the inferred schema.
/// </summary>
internal static class ObjectSchemaInferrer
{
    /// <summary>
    /// Represents everything we know about a single property
    /// after observing it across all instances in the array.
    /// </summary>
    internal class InferredProperty
    {
        public HashSet<JTokenType> ObservedTypes { get; } = new();
        public bool IsNullableOrMissing { get; set; } = false;
        public List<JToken> NonNullSamples { get; } = new();
    }

    /// <summary>
    /// Walks all objects in the array and returns a merged property map.
    /// Keys are the original JSON property names (unsanitized).
    /// </summary>
    public static Dictionary<string, InferredProperty> Infer(
        JArray array,
        GenerationOptions options)
    {
        var schema = new Dictionary<string, InferredProperty>();

        var allPropertyNames = array
            .OfType<JObject>()
            .SelectMany(obj => obj.Properties().Select(p => p.Name))
            .Distinct()
            .ToList();

        foreach (var name in allPropertyNames)
            schema[name] = new InferredProperty();

        foreach (var token in array)
        {
            if (token is not JObject obj)
                continue;

            foreach (var name in allPropertyNames)
            {
                var prop = schema[name];
                var value = obj[name]; // null if property is absent

                if (value == null || value.Type == JTokenType.Null)
                {
                    prop.IsNullableOrMissing = true;
                }
                else
                {
                    prop.ObservedTypes.Add(value.Type);
                    prop.NonNullSamples.Add(value);
                }
            }
        }

        return schema;
    }

    /// <summary>
    /// Determines the final C# type string for an inferred property.
    /// Applies the merging decision table:
    ///   - One consistent type, never null  → type
    ///   - One consistent type, sometimes null → type?
    ///   - Only nulls seen                  → object? + TODO
    ///   - Conflicting types                → object? + TODO conflict
    /// </summary>
    public static (string CsharpType, string? Comment) ResolveType(
        InferredProperty prop,
        GenerationOptions options)
    {
        if (prop.ObservedTypes.Count == 0)
            return (
                options.UseNullableReferenceTypes ? "object?" : "object",
                "// TODO: replace 'object' with the actual type — only null values observed"
            );
        if (prop.ObservedTypes.Count > 1)
        {
            var seen = string.Join(", ", prop.ObservedTypes);
            return (
                options.UseNullableReferenceTypes ? "object?" : "object",
                $"// TODO: conflicting types observed ({seen}) — verify manually"
            );
        }

        var sample = prop.NonNullSamples.First();
        var csharpType = TypeResolver.Resolve(sample, options);

        if (prop.IsNullableOrMissing)
        {
            if (!csharpType.EndsWith('?'))
                csharpType += "?";
        }

        return (csharpType, null);
    }
}