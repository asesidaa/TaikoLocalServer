namespace TaikoLocalServer.Adapters.GameProtocol.CnR00;

public static class DependencyInjection
{
    public static IServiceCollection AddGameProtocolCnR00(this IServiceCollection services)
    {
        // Controllers picked up via assembly scanning. Future-hook for per-version services.
        return Shared.Cabinets.CabinetFilter.AddOnce(services);
    }
}
