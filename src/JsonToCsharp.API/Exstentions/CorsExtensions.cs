namespace JsonToCsharp.API.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("AllowedOrigins", policy =>
            {
                if (allowedOrigins.Contains("*"))
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                }
                else
                {
                    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
                }
            });
        });

        return services;
    }
}