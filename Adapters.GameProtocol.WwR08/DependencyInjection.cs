namespace TaikoLocalServer.Adapters.GameProtocol.WwR08;

public static class DependencyInjection
{
    public static IServiceCollection AddGameProtocolWwR08(this IServiceCollection services)
    {
        // Controllers picked up via assembly scanning. Future-hook for per-version services.
        return services;
    }
}
