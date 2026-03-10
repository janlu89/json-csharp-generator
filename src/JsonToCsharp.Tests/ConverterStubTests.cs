using JsonToCsharp.Engine.Models;
using JsonToCsharp.Engine.Services;

namespace JsonToCsharp.Tests;

public class ConverterStubTests
{
    [Fact]
    public void JsonToCsharp_StubReturnsSuccess()
    {
        // Arrange
        var converter = new JsonToCsharpConverter();
        var options = new GenerationOptions();

        // Act
        var result = converter.Convert("{}", options);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Output);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void CsharpToJson_StubReturnsSuccess()
    {
        //Arrange
        var converter = new CsharpToJsonConverter();

        //Act
        var result = converter.Convert("public class Foo {}");

        //Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Output);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ConversionResult_OkFactory_SetsPropertiesCorrectly()
    {
        var result = ConversionResult.Ok("some output");

        Assert.True(result.Success);
        Assert.Equal("some output", result.Output);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ConversionResult_FailFactory_SetsPropertiesCorrectly()
    {
        var result = ConversionResult.Fail("something went wrong");

        Assert.False(result.Success);
        Assert.Null(result.Output);
        Assert.Equal("something went wrong", result.ErrorMessage);
    }
}