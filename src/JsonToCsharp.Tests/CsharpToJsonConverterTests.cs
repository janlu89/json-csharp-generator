using JsonToCsharp.Engine.Services;

namespace JsonToCsharp.Tests;

public class CsharpToJsonConverterTests
{
    private readonly CsharpToJsonConverter _converter = new();

    // --- Input validation ---

    [Fact]
    public void Convert_EmptyString_ReturnsFailure()
    {
        var result = _converter.Convert("");
        Assert.False(result.Success);
    }

    [Fact]
    public void Convert_NoClassFound_ReturnsFailure()
    {
        var result = _converter.Convert("this is not csharp");
        Assert.False(result.Success);
    }

    // --- Primitive types ---

    [Fact]
    public void Convert_StringProperty_EmitsStringPlaceholder()
    {
        var result = _converter.Convert(
            "public class Foo { public string Name { get; set; } }");
        Assert.True(result.Success);
        Assert.Contains("\"name\"", result.Output);
        Assert.Contains("\"string\"", result.Output);
    }

    [Fact]
    public void Convert_IntProperty_EmitsZero()
    {
        var result = _converter.Convert(
            "public class Foo { public int Age { get; set; } }");
        Assert.True(result.Success);
        Assert.Contains("\"age\"", result.Output);
        Assert.Contains("0", result.Output);
    }

    [Fact]
    public void Convert_BoolProperty_EmitsFalse()
    {
        var result = _converter.Convert(
            "public class Foo { public bool Active { get; set; } }");
        Assert.True(result.Success);
        Assert.Contains("\"active\"", result.Output);
        Assert.Contains("false", result.Output);
    }

    [Fact]
    public void Convert_DoubleProperty_EmitsDecimalZero()
    {
        var result = _converter.Convert(
            "public class Foo { public double Score { get; set; } }");
        Assert.True(result.Success);
        Assert.Contains("0.0", result.Output);
    }

    [Fact]
    public void Convert_DateTimeProperty_EmitsIsoString()
    {
        var result = _converter.Convert(
            "public class Foo { public DateTime CreatedAt { get; set; } }");
        Assert.True(result.Success);
        Assert.Contains("2024-01-01", result.Output);
    }

    [Fact]
    public void Convert_GuidProperty_EmitsEmptyGuid()
    {
        var result = _converter.Convert(
            "public class Foo { public Guid Id { get; set; } }");
        Assert.True(result.Success);
        Assert.Contains("00000000-0000-0000-0000-000000000000", result.Output);
    }

    [Fact]
    public void Convert_NullableProperty_EmitsCorrectValue()
    {
        var result = _converter.Convert(
            "public class Foo { public int? Age { get; set; } }");
        Assert.True(result.Success);
        Assert.Contains("\"age\"", result.Output);
        Assert.Contains("0", result.Output);
    }

    // --- Collection types ---

    [Fact]
    public void Convert_ListOfString_EmitsArrayWithElement()
    {
        var result = _converter.Convert(
            "public class Foo { public List<string> Tags { get; set; } }");
        Assert.True(result.Success);
        Assert.Contains("\"tags\"", result.Output);
        Assert.Contains("[", result.Output);
        Assert.Contains("\"string\"", result.Output);
    }

    [Fact]
    public void Convert_ListOfInt_EmitsArrayWithZero()
    {
        var result = _converter.Convert(
            "public class Foo { public List<int> Scores { get; set; } }");
        Assert.True(result.Success);
        Assert.Contains("[", result.Output);
        Assert.Contains("0", result.Output);
    }

    [Fact]
    public void Convert_ArraySyntax_EmitsArray()
    {
        var result = _converter.Convert(
            "public class Foo { public string[] Names { get; set; } }");
        Assert.True(result.Success);
        Assert.Contains("[", result.Output);
    }

    // --- Nested classes ---

    [Fact]
    public void Convert_NestedClass_EmitsNestedObject()
    {
        var code = """
            public class Root
            {
                public string Name { get; set; }
                public Address Address { get; set; }
            }
            public class Address
            {
                public string City { get; set; }
                public string Zip { get; set; }
            }
            """;

        var result = _converter.Convert(code);
        Assert.True(result.Success);
        Assert.Contains("\"address\"", result.Output);
        Assert.Contains("\"city\"", result.Output);
        Assert.Contains("\"zip\"", result.Output);
    }

    [Fact]
    public void Convert_UnknownNestedType_EmitsEmptyObject()
    {
        var result = _converter.Convert(
            "public class Foo { public SomeUnknownType Data { get; set; } }");
        Assert.True(result.Success);
        Assert.Contains("\"data\"", result.Output);
        Assert.Contains("{}", result.Output);
    }

    // --- Circular reference protection ---

    [Fact]
    public void Convert_CircularReference_DoesNotStackOverflow()
    {
        var code = """
            public class Node
            {
                public int Value { get; set; }
                public Node Next { get; set; }
            }
            """;

        var result = _converter.Convert(code);
        Assert.True(result.Success);
        Assert.Contains("\"value\"", result.Output);

        Assert.Contains("null", result.Output);
    }

    // --- Record support ---

    [Fact]
    public void Convert_Record_ParsesCorrectly()
    {
        var result = _converter.Convert(
            "public record Person { public string Name { get; init; } }");
        Assert.True(result.Success);
        Assert.Contains("\"name\"", result.Output);
        Assert.Contains("\"string\"", result.Output);
    }

    // --- Output format ---

    [Fact]
    public void Convert_Output_IsValidJson()
    {
        var result = _converter.Convert(
            "public class Foo { public string Name { get; set; } public int Age { get; set; } }");
        Assert.True(result.Success);

        var parsed = System.Text.Json.JsonDocument.Parse(result.Output!);
        Assert.NotNull(parsed);
    }

    [Fact]
    public void Convert_Output_IsIndented()
    {
        var result = _converter.Convert(
            "public class Foo { public string Name { get; set; } }");
        Assert.True(result.Success);

        Assert.Contains("\n", result.Output);
    }
}