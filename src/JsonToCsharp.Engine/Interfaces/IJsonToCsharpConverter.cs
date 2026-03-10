using JsonToCsharp.Engine.Models;

namespace JsonToCsharp.Engine.Interfaces;

public interface IJsonToCsharpConverter
{
    ConversionResult Convert(string json, GenerationOptions options);
}