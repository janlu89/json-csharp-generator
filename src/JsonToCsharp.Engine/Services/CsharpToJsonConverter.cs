using JsonToCsharp.Engine.Interfaces;
using JsonToCsharp.Engine.Models;

namespace JsonToCsharp.Engine.Services;

public class CsharpToJsonConverter : ICsharpToJsonConverter
{
    public ConversionResult Convert(string csharpCode)
    {
        return ConversionResult.Ok("{}");
    }
}