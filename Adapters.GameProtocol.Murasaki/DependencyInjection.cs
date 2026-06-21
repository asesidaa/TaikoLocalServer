using Microsoft.Extensions.DependencyInjection;

namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki;

public static class DependencyInjection
{
    public const GameEra Era = GameEra.Murasaki;

    public static IServiceCollection AddGameProtocolMurasaki(this IServiceCollection services) => services;
}
