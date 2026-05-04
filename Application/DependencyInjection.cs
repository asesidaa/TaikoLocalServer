using Microsoft.Extensions.DependencyInjection;

namespace TaikoLocalServer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(opt =>
        {
            opt.Namespace = "TaikoLocalServer.Application";
            opt.ServiceLifetime = ServiceLifetime.Scoped;
        });

        return services;
    }
}
