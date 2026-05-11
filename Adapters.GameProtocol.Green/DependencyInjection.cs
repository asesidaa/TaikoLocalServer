namespace TaikoLocalServer.Adapters.GameProtocol.Green;

public static class DependencyInjection
{
    public const GameEra Era = GameEra.Green;

    public static IServiceCollection AddGameProtocolGreen(this IServiceCollection services)
    {
        return services;
    }
}
