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
    public void Convert_RootIsArrayOfPrimitives_ReturnsSuccess()
    {
        var result = _converter.Convert("[1, 2, 3]", _default);
        Assert.True(result.Success);
        Assert.Contains("List<int>", result.Output);
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

    // --- Nested objects ---

    [Fact]
    public void Convert_NestedObject_GeneratesTwoClasses()
    {
        var json = """
            {
                "name": "John",
                "address": {
                    "city": "Rome",
                    "zip": "00100"
                }
            }
            """;

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        Assert.Contains("public class Root", result.Output);
        Assert.Contains("public class Address", result.Output);
    }

    [Fact]
    public void Convert_NestedObject_NestedClassAppearsBeforeParent()
    {
        var json = """
            {
                "name": "John",
                "address": {"city": "Rome"}
            }
            """;

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);

        // Address must be defined before Root so the compiler can resolve the type
        var addressIndex = result.Output!.IndexOf("public class Address");
        var rootIndex = result.Output.IndexOf("public class Root");
        Assert.True(addressIndex < rootIndex);
    }

    [Fact]
    public void Convert_NestedObject_PropertyTypeUsesClassName()
    {
        var json = """{"address": {"city": "Rome"}}""";

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        Assert.Contains("public Address Address", result.Output);
    }

    [Fact]
    public void Convert_DeeplyNestedObject_GeneratesAllClasses()
    {
        var json = """
            {
                "user": {
                    "name": "John",
                    "location": {
                        "city": "Rome",
                        "country": "Italy"
                    }
                }
            }
            """;

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        Assert.Contains("public class Root", result.Output);
        Assert.Contains("public class User", result.Output);
        Assert.Contains("public class Location", result.Output);
    }

    // --- Arrays of primitives ---

    [Fact]
    public void Convert_ArrayOfStrings_EmitsListOfString()
    {
        var json = """{"tags": ["developer", "dotnet"]}""";

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        Assert.Contains("List<string>", result.Output);
    }

    [Fact]
    public void Convert_ArrayOfIntegers_EmitsListOfInt()
    {
        var json = """{"scores": [1, 2, 3]}""";

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        Assert.Contains("List<int>", result.Output);
    }

    [Fact]
    public void Convert_EmptyArray_EmitsListOfObjectWithTodo()
    {
        var json = """{"items": []}""";

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        Assert.Contains("List<object>", result.Output);
        Assert.Contains("TODO", result.Output);
    }

    [Fact]
    public void Convert_MixedTypeArray_EmitsListOfObjectWithComment()
    {
        var json = """{"mixed": [1, "hello", true]}""";

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        Assert.Contains("List<object>", result.Output);
        Assert.Contains("TODO", result.Output);
    }

    // --- Arrays of objects ---

    [Fact]
    public void Convert_ArrayOfObjects_GeneratesElementClass()
    {
        var json = """
            {
                "addresses": [
                    {"city": "Rome"},
                    {"city": "Milan"}
                ]
            }
            """;

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        // "addresses" → singularized to "Address"
        Assert.Contains("public class Address", result.Output);
        Assert.Contains("List<Address>", result.Output);
    }

    // --- Root array handling ---

    [Fact]
    public void Convert_RootArray_GeneratesItemClassAndComment()
    {
        var json = """
            [
                {"id": 1, "name": "John"},
                {"id": 2, "name": "Jane"}
            ]
            """;

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        Assert.Contains("public class Item", result.Output);
        Assert.Contains("// Root type: List<Item>", result.Output);
    }

    [Fact]
    public void Convert_RootArray_CustomItemClassName_UsesProvidedName()
    {
        var json = """[{"id": 1, "name": "John"}]""";
        var options = new GenerationOptions { ArrayItemClassName = "Person" };

        var result = _converter.Convert(json, options);

        Assert.True(result.Success);
        Assert.Contains("public class Person", result.Output);
        Assert.Contains("// Root type: List<Person>", result.Output);
    }

    [Fact]
    public void Convert_EmptyRootArray_ReturnsFailure()
    {
        var result = _converter.Convert("[]", _default);
        Assert.False(result.Success);
    }

    // --- Schema merging ---

    [Fact]
    public void Convert_RootArray_NullInSomeInstances_PropertyIsNullable()
    {
        var json = """
            [
                {"id": 1, "email": null},
                {"id": 2, "email": "jane@example.com"},
                {"id": 3, "email": "bob@example.com"}
            ]
            """;

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        // email is string in some, null in others → must be string?
        Assert.Contains("string?", result.Output);
    }

    [Fact]
    public void Convert_RootArray_MissingPropertyInSomeInstances_IsNullable()
    {
        // "name" is absent in the second instance — absence == nullable
        var json = """
            [
                {"id": 1, "name": "John"},
                {"id": 2}
            ]
            """;

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        Assert.Contains("string?", result.Output);
        Assert.Contains("int", result.Output);
    }

    [Fact]
    public void Convert_RootArray_ConsistentTypeNeverNull_IsNotNullable()
    {
        var json = """
            [
                {"id": 1, "active": true},
                {"id": 2, "active": false},
                {"id": 3, "active": true}
            ]
            """;

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        // active is always bool, never null → bool not bool?
        Assert.Contains("bool ", result.Output);
        Assert.DoesNotContain("bool?", result.Output);
    }

    [Fact]
    public void Convert_RootArray_ConflictingTypes_EmitsObjectWithComment()
    {
        var json = """
            [
                {"value": 1},
                {"value": "hello"}
            ]
            """;

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        Assert.Contains("object?", result.Output);
        Assert.Contains("conflicting", result.Output);
    }

    [Fact]
    public void Convert_RootArray_AllNullProperty_EmitsObjectWithTodo()
    {
        var json = """
            [
                {"id": 1, "data": null},
                {"id": 2, "data": null}
            ]
            """;

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        Assert.Contains("object?", result.Output);
        Assert.Contains("TODO", result.Output);
    }

    // --- Naming collision ---

    [Fact]
    public void Convert_NamingCollision_SecondClassGetsSuffix()
    {
        var json = """
            {
                "address": {"city": "Rome"},
                "Address": {"city": "Milan"}
            }
            """;

        var result = _converter.Convert(json, _default);

        Assert.True(result.Success);
        Assert.Contains("public class Address", result.Output);
        Assert.Contains("public class Address2", result.Output);
    }

    [Fact]
    public void Convert_AttributeStyleNone_ButNameWasSanitized_StillEmitsAttribute()
    {
        var options = new GenerationOptions { AttributeStyle = AttributeStyle.None };
        var result = _converter.Convert("""{"first-name": "John"}""", options);

        Assert.True(result.Success);

        Assert.Contains("JsonPropertyName", result.Output);
    }

    [Fact]
    public void Convert_AttributeStyleNone_CleanName_NoAttributeEmitted()
    {
        var options = new GenerationOptions { AttributeStyle = AttributeStyle.None };
        var result = _converter.Convert("""{"name": "John"}""", options);

        Assert.True(result.Success);

        Assert.DoesNotContain("JsonPropertyName", result.Output);
    }

    [Fact]
    public void Convert_GenerateAsRecord_UsesInitAccessor()
    {
        var options = new GenerationOptions { GenerateAsRecord = true };
        var result = _converter.Convert("""{"name": "John"}""", options);

        Assert.True(result.Success);
        Assert.Contains("get; init;", result.Output);
        Assert.DoesNotContain("get; set;", result.Output);
    }

    [Fact]
    public void Convert_SystemTextJsonStyle_PrependsUsingDirective()
    {
        var options = new GenerationOptions { AttributeStyle = AttributeStyle.SystemTextJson };
        var result = _converter.Convert("""{"name": "John"}""", options);
        Assert.True(result.Success);
        Assert.StartsWith("using System.Text.Json.Serialization;", result.Output);
    }

    [Fact]
    public void Convert_NewtonsoftStyle_PrependsNewtonsoftUsing()
    {
        var options = new GenerationOptions { AttributeStyle = AttributeStyle.Newtonsoft };
        var result = _converter.Convert("""{"name": "John"}""", options);
        Assert.True(result.Success);
        Assert.StartsWith("using Newtonsoft.Json;", result.Output);
    }

    [Fact]
    public void Convert_AttributeStyleNone_NoUsingDirective()
    {
        var options = new GenerationOptions { AttributeStyle = AttributeStyle.None };
        var result = _converter.Convert("""{"name": "John"}""", options);
        Assert.True(result.Success);
        Assert.DoesNotContain("using ", result.Output);
    }
}