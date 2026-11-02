namespace TaikoLocalServer.Adapters.GameProtocol.White;

public static class DependencyInjection
{
    public const GameEra Era = GameEra.White;

    public static IServiceCollection AddGameProtocolWhite(this IServiceCollection services)
    {
        return services;
    }
}
