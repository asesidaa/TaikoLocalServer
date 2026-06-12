namespace TaikoLocalServer.Adapters.GameProtocol.Red;

public static class DependencyInjection
{
    public const GameEra Era = GameEra.Red;

    public static IServiceCollection AddGameProtocolRed(this IServiceCollection services)
    {
        return services;
    }
}
