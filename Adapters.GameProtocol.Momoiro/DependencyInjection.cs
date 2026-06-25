using Microsoft.Extensions.DependencyInjection;

namespace TaikoLocalServer.Adapters.GameProtocol.Momoiro;

public static class DependencyInjection
{
    public const GameEra Era = GameEra.Momoiro;

    public static IServiceCollection AddGameProtocolMomoiro(this IServiceCollection services) => services;
}
