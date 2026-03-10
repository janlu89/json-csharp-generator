using JsonToCsharp.Engine.Interfaces;
using JsonToCsharp.Engine.Models;

namespace JsonToCsharp.Engine.Services;

public class JsonToCsharpConverter : IJsonToCsharpConverter
{
    public ConversionResult Convert(string json, GenerationOptions options)
    {
        // Stub — real implementation coming..
        return ConversionResult.Ok("// Generated C# will appear here");
    }
}