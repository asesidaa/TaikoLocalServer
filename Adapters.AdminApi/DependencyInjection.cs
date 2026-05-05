using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TaikoLocalServer.Adapters.AdminApi;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminApi(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
