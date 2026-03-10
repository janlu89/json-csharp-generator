using JsonToCsharp.Engine.Models;
using JsonToCsharp.Engine.Services;

namespace JsonToCsharp.Tests;

public class JsonToCsharpConverterTests
{
    private readonly GenerationOptions _default = new();
    private readonly JsonToCsharpConverter _converter = new();

    // --- Input validation ---

    [Fact]
    public void Convert_EmptyString_ReturnsFailure()
    {
        var result = _converter.Convert("", _default);
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Convert_InvalidJson_ReturnsFailure()
    {
        var result = _converter.Convert("this is not json", _default);
        Assert.False(result.Success);
        Assert.Contains("Invalid JSON", result.ErrorMessage);
    }

    [Fact]
    public void Convert_RootIsArray_ReturnsFailure()
    {
        var result = _converter.Convert("[1, 2, 3]", _default);
        Assert.False(result.Success);
    }

    // --- Primitive type mapping (simple mode) ---

    [Fact]
    public void Convert_StringProperty_EmitsNullableString()
    {
        var result = _converter.Convert("""{"name": "John"}""", _default);
        Assert.True(result.Success);
        Assert.Contains("string?", result.Output);
    }

    [Fact]
    public void Convert_BoolProperty_EmitsBool()
    {
        var result = _converter.Convert("""{"active": true}""", _default);
        Assert.True(result.Success);
        Assert.Contains("bool", result.Output);
    }

    [Fact]
    public void Convert_IntProperty_EmitsInt()
    {
        var result = _converter.Convert("""{"age": 30}""", _default);
        Assert.True(result.Success);
        Assert.Contains("int", result.Output);
    }

    [Fact]
    public void Convert_LargeNumber_SimpleMode_StillEmitsInt()
    {
        var result = _converter.Convert("""{"bigNumber": 9999999999}""", _default);
        Assert.True(result.Success);
        Assert.Contains("int", result.Output);
    }

    [Fact]
    public void Convert_DecimalNumber_EmitsDouble()
    {
        var result = _converter.Convert("""{"price": 9.99}""", _default);
        Assert.True(result.Success);
        Assert.Contains("double", result.Output);
    }

    [Fact]
    public void Convert_NullProperty_EmitsNullableObject()
    {
        var result = _converter.Convert("""{"data": null}""", _default);
        Assert.True(result.Success);
        Assert.Contains("object?", result.Output);
        Assert.Contains("TODO", result.Output);
    }

    // --- Precise mode ---

    [Fact]
    public void Convert_DateString_PreciseMode_EmitsDateTime()
    {
        var options = new GenerationOptions { UsePreciseTypes = true };
        var result = _converter.Convert("""{"createdAt": "2024-01-15T10:30:00Z"}""", options);
        Assert.True(result.Success);
        Assert.Contains("DateTime", result.Output);
    }

    [Fact]
    public void Convert_GuidString_PreciseMode_EmitsGuid()
    {
        var options = new GenerationOptions { UsePreciseTypes = true };
        var result = _converter.Convert(
            """{"id": "d3b07384-d9a0-4b9f-8f8e-123456789012"}""", options);
        Assert.True(result.Success);
        Assert.Contains("Guid", result.Output);
    }

    [Fact]
    public void Convert_LargeNumber_PreciseMode_EmitsLong()
    {
        var options = new GenerationOptions { UsePreciseTypes = true };
        var result = _converter.Convert("""{"bigNumber": 9999999999}""", options);
        Assert.True(result.Success);
        Assert.Contains("long", result.Output);
    }

    // --- Options ---

    [Fact]
    public void Convert_NullableDisabled_StringHasNoQuestionMark()
    {
        var options = new GenerationOptions { UseNullableReferenceTypes = false };
        var result = _converter.Convert("""{"name": "John"}""", options);
        Assert.True(result.Success);
        Assert.DoesNotContain("string?", result.Output);
        Assert.Contains("string ", result.Output);
    }

    [Fact]
    public void Convert_CamelCase_PropertyStartsLowercase()
    {
        var options = new GenerationOptions { NamingConvention = NamingConvention.CamelCase };
        var result = _converter.Convert("""{"FirstName": "John"}""", options);
        Assert.True(result.Success);
        Assert.Contains("firstName", result.Output);
    }

    [Fact]
    public void Convert_CustomRootClassName_UsesProvidedName()
    {
        var options = new GenerationOptions { RootClassName = "Person" };
        var result = _converter.Convert("""{"name": "John"}""", options);
        Assert.True(result.Success);
        Assert.Contains("class Person", result.Output);
    }

    [Fact]
    public void Convert_GenerateAsRecord_EmitsRecord()
    {
        var options = new GenerationOptions { GenerateAsRecord = true };
        var result = _converter.Convert("""{"name": "John"}""", options);
        Assert.True(result.Success);
        Assert.Contains("record", result.Output);
        Assert.DoesNotContain("class", result.Output);
    }

    // --- Name sanitization ---

    [Fact]
    public void Convert_HyphenatedName_NormalisesToPascalCase()
    {
        var result = _converter.Convert("""{"first-name": "John"}""", _default);
        Assert.True(result.Success);
        Assert.Contains("FirstName", result.Output);
    }

    [Fact]
    public void Convert_PropertyStartingWithDigit_IsPrefixed()
    {
        var result = _converter.Convert("""{"2fa": true}""", _default);
        Assert.True(result.Success);
        Assert.Contains("_2fa", result.Output);
    }

    // --- Real-world dirty JSON (Newtonsoft forgiveness) ---

    [Fact]
    public void Convert_TrailingComma_ParsesSuccessfully()
    {
        var result = _converter.Convert("""{"name": "John",}""", _default);
        Assert.True(result.Success);
    }

    // --- Full flat object integration ---

    [Fact]
    public void Convert_FlatObject_GeneratesCompleteClass()
    {
        var json = """
            {
                "name": "John",
                "age": 30,
                "active": true,
                "score": 9.5,
                "data": null
            }
            """;

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        Assert.Contains("public class Root", result.Output);
        Assert.Contains("string?", result.Output);
        Assert.Contains("int", result.Output);
        Assert.Contains("bool", result.Output);
        Assert.Contains("double", result.Output);
        Assert.Contains("object?", result.Output);
        Assert.Contains("TODO", result.Output);
    }
}