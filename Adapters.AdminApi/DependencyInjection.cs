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

    public static IServiceCollection AddAdminApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            // 10 attempts per minute per client IP, shared across all auth endpoints.
            options.AddPolicy(AuthRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
        });
        return services;
    }

    public static WebApplication UseAdminApi(this WebApplication app)
    {
        app.UseRateLimiter();
        return app;
    }
}
