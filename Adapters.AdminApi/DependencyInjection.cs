using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TaikoLocalServer.Adapters.AdminApi;

public static class DependencyInjection
{
    /// <summary>Rate-limit policy for login/register/password endpoints (brute-force guard).</summary>
    public const string AuthRateLimitPolicy = "auth";

    /// <summary>Rate-limit policy for game assets: plenty for browsing, slow for bulk scraping.</summary>
    public const string AssetsRateLimitPolicy = "assets";

    public static IServiceCollection AddAdminApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            // 10 attempts per minute per client IP, shared across all auth endpoints.
            options.AddPolicy(AuthRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
            // A Profile visit is ~20 requests (the browser caches them for a day); all ~700 extracted files take minutes.
            options.AddPolicy(AssetsRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 300, Window = TimeSpan.FromMinutes(1) }));
        });
        return services;
    }

    public static WebApplication UseAdminApi(this WebApplication app)
    {
        app.UseRateLimiter();
        return app;
    }
}
