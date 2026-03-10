namespace JsonToCsharp.Engine.Models;

public class ConversionResult
{
    public bool Success { get; init; }
    public string? Output { get; init; }
    public string? ErrorMessage { get; init; }

    public static ConversionResult Ok(string output) =>
        new() { Success = true, Output = output };

    public static ConversionResult Fail(string error) =>
        new() { Success = false, ErrorMessage = error };
}