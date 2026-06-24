using Microsoft.Extensions.DependencyInjection;

namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori;

public static class DependencyInjection
{
    public const GameEra Era = GameEra.Kimidori;

    public static IServiceCollection AddGameProtocolKimidori(this IServiceCollection services) => services;
}
