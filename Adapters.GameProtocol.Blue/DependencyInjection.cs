namespace TaikoLocalServer.Adapters.GameProtocol.Blue;

public static class DependencyInjection
{
    public const GameEra Era = GameEra.Blue;

    public static IServiceCollection AddGameProtocolBlue(this IServiceCollection services)
    {
        return services;
    }
}
