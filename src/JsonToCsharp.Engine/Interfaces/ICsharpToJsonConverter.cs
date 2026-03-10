using JsonToCsharp.Engine.Models;

namespace JsonToCsharp.Engine.Interfaces;

public interface ICsharpToJsonConverter
{
    ConversionResult Convert(string csharpCode);
}