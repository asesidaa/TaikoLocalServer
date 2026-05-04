using Microsoft.AspNetCore.Builder;
using TaikoLocalServer.Adapters.AllnetMucha.Middleware;

namespace TaikoLocalServer.Adapters.AllnetMucha;

public static class DependencyInjection
{
    public static IServiceCollection AddAllnetMucha(this IServiceCollection services)
    {
        // Controllers + middleware are picked up via assembly scanning + UseAllnetMucha().
        // No DI registrations specific to AllnetMucha at present.
        return services;
    }

    public static WebApplication UseAllnetMucha(this WebApplication app)
    {
        app.UseWhen(
            context => context.Request.Path.StartsWithSegments("/sys/servlet/PowerOn", StringComparison.InvariantCulture),
            applicationBuilder => applicationBuilder.UseAllNetRequestMiddleware());

        return app;
    }
}
