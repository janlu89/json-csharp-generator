namespace JsonToCsharp.API.Extensions;

public static class RequestValidationExtensions
{
    private const int MaxInputSizeBytes = 1_000_000; // 1MB

    /// <summary>
    /// Validates that the input string is not null, empty, or exceeding
    /// the maximum allowed size. Returns a BadRequest result if invalid,
    /// or null if the input is acceptable.
    /// </summary>
    public static IResult? ValidateInput(this string? input, string fieldName = "input")
    {
        if (string.IsNullOrWhiteSpace(input))
            return Results.BadRequest(new { error = $"The {fieldName} field cannot be empty." });

        if (input.Length > MaxInputSizeBytes)
            return Results.BadRequest(new { error = $"The {fieldName} field exceeds the maximum allowed size of 1MB." });

        return null;
    }
}