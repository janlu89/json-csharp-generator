using JsonToCsharp.API.Endpoints;
using JsonToCsharp.API.Extensions;
using JsonToCsharp.Engine.Interfaces;
using JsonToCsharp.Engine.Models;
using JsonToCsharp.Engine.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Dependency Injection ---
builder.Services.AddSingleton<IJsonToCsharpConverter, JsonToCsharpConverter>();
builder.Services.AddSingleton<ICsharpToJsonConverter, CsharpToJsonConverter>();

builder.Services.AddHttpClient("fetchClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("User-Agent", "JsonCsharpGenerator/1.0");
});

// --- CORS ---
builder.Services.AddCorsPolicy(builder.Configuration);

// --- Health Checks ---
builder.Services.AddHealthChecks();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter());
});

var app = builder.Build();

// --- Middleware ---
app.UseCors("AllowedOrigins");

// --- Health Check Endpoint ---
app.MapHealthChecks("/health");

// --- Endpoints ---
app.MapConversionEndpoints();

app.Run();

// --- Request Types ---
record JsonToCsharpRequest(string? Json, GenerationOptions? Options);
record CsharpToJsonRequest(string? CsharpCode);
record FetchJsonRequest(string? Url);