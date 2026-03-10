using JsonToCsharp.API.Extensions;
using JsonToCsharp.Engine.Interfaces;
using JsonToCsharp.Engine.Models;

namespace JsonToCsharp.API.Endpoints;

public static class ConversionEndpoints
{
    public static WebApplication MapConversionEndpoints(this WebApplication app)
    {
        app.MapPost("/api/json-to-csharp", (
            JsonToCsharpRequest req,
            IJsonToCsharpConverter converter) =>
        {
            var validation = req.Json.ValidateInput("json");
            if (validation != null) return validation;

            var result = converter.Convert(req.Json!, req.Options ?? new GenerationOptions());
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        app.MapPost("/api/csharp-to-json", (
            CsharpToJsonRequest req,
            ICsharpToJsonConverter converter) =>
        {
            var validation = req.CsharpCode.ValidateInput("csharpCode");
            if (validation != null) return validation;

            var result = converter.Convert(req.CsharpCode!);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        });

        app.MapPost("/api/fetch-json", async (
            FetchJsonRequest req,
            IHttpClientFactory httpFactory) =>
        {
            var validation = req.Url.ValidateInput("url");
            if (validation != null) return validation;

            // TODO: implement real URL fetching on Day 6
            await Task.CompletedTask;
            return Results.Ok(new { json = "" });
        });

        return app;
    }
}