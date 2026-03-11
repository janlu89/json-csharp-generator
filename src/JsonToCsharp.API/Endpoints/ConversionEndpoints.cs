using JsonToCsharp.API.Extensions;
using JsonToCsharp.Engine.Interfaces;
using JsonToCsharp.Engine.Models;
using System.Net;
using System.Text.Json;

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

            if (!Uri.TryCreate(req.Url, UriKind.Absolute, out var uri))
                return Results.BadRequest(new { error = "The provided URL is not valid." });

            if (uri.Scheme != "http" && uri.Scheme != "https")
                return Results.BadRequest(new
                { error = "Only http and https URLs are supported." });

            if (SsrfGuard.IsPrivateAddress(uri.Host))
                return Results.BadRequest(new
                { error = "Requests to private or loopback addresses are not allowed." });

            try
            {
                var client = httpFactory.CreateClient("fetchClient");
                var response = await client.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                    return Results.BadRequest(new
                    {
                        error = $"The URL returned HTTP {(int)response.StatusCode} " +
                                $"{response.StatusCode}."
                    });

                var content = await response.Content.ReadAsStringAsync();

                if (content.Length > 1_000_000)
                    return Results.BadRequest(new
                    { error = "The fetched content exceeds the maximum allowed size of 1MB." });

                try
                {
                    JsonDocument.Parse(content);
                }
                catch (JsonException)
                {
                    return Results.BadRequest(new
                    { error = "The URL did not return valid JSON content." });
                }

                return Results.Ok(new { json = content });
            }
            catch (TaskCanceledException)
            {
                return Results.BadRequest(new
                { error = "The request timed out after 10 seconds." });
            }
            catch (HttpRequestException ex)
            {
                return Results.BadRequest(new
                { error = $"Could not reach the URL: {ex.Message}" });
            }
        });

        // Health check — used by Render.com to verify the container is ready
        // and by the GitHub Actions keep-warm workflow to prevent cold starts.
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

        return app;
    }
}

// -------------------------------------------------------------------------
// SSRF Guard — prevents the server from being used to probe internal
// network resources on behalf of a user-supplied URL.
// -------------------------------------------------------------------------
internal static class SsrfGuard
{
    public static bool IsPrivateAddress(string host)
    {
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return true;

        if (!IPAddress.TryParse(host, out var ip))
        {
            try
            {
                var addresses = Dns.GetHostAddresses(host);
                ip = addresses.FirstOrDefault();
            }
            catch
            {
                return false;
            }
        }

        if (ip == null) return false;

        var bytes = ip.GetAddressBytes();

        // IPv4 private ranges:
        // 127.0.0.0/8   — loopback
        // 10.0.0.0/8    — private class A
        // 172.16.0.0/12 — private class B
        // 192.168.0.0/16 — private class C
        if (bytes.Length == 4)
        {
            return bytes[0] == 127 ||
                   bytes[0] == 10 ||
                   (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                   (bytes[0] == 192 && bytes[1] == 168);
        }

        // IPv6 loopback ::1
        if (bytes.Length == 16)
            return ip.Equals(IPAddress.IPv6Loopback);

        return false;
    }
}