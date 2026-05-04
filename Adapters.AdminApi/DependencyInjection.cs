using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Filters;

namespace TaikoLocalServer.Adapters.AdminApi;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuthorizeIfRequiredAttribute>();

        // JWT scheme registration stays in Infrastructure for PR3 (CONTINUATION deviation #6).
        // PR4 may move it here, alongside Contracts.AdminApi.

        return services;
    }
}
