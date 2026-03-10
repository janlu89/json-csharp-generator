using JsonToCsharp.API.Extensions;
using JsonToCsharp.Engine.Interfaces;
using JsonToCsharp.Engine.Models;
using JsonToCsharp.Engine.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Dependency Injection ---
builder.Services.AddSingleton<IJsonToCsharpConverter, JsonToCsharpConverter>();
builder.Services.AddSingleton<ICsharpToJsonConverter, CsharpToJsonConverter>();

builder.Services.AddHttpClient();

// --- CORS ---
builder.Services.AddCorsPolicy(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowedOrigins");

// --- Endpoints ---

// Endpoint 1: JSON → C#
app.MapPost("/api/json-to-csharp", (JsonToCsharpRequest req, IJsonToCsharpConverter converter) =>
{
    var result = converter.Convert(req.Json, req.Options ?? new GenerationOptions());
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// Endpoint 2: C# → JSON
app.MapPost("/api/csharp-to-json", (CsharpToJsonRequest req, ICsharpToJsonConverter converter) =>
{
    var result = converter.Convert(req.CsharpCode);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// Endpoint 3: Fetch JSON from a URL
app.MapPost("/api/fetch-json", async (FetchJsonRequest req, IHttpClientFactory httpFactory) =>
{
    // TODO: implement real URL fetching on Day 6
    await Task.CompletedTask;
    return Results.Ok(new { json = "" });
});

app.Run();

// --- Request types ---
record JsonToCsharpRequest(string Json, GenerationOptions? Options);
record CsharpToJsonRequest(string CsharpCode);
record FetchJsonRequest(string Url);