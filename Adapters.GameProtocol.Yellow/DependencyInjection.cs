namespace TaikoLocalServer.Adapters.GameProtocol.Yellow;

public static class DependencyInjection
{
    public const GameEra Era = GameEra.Yellow;

    public static IServiceCollection AddGameProtocolYellow(this IServiceCollection services)
    {
        return services;
    }
}
