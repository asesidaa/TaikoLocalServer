using Microsoft.Extensions.Options;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Settings;

public static class ServerSettingsOptionsValidationExtensions
{
    public static OptionsBuilder<ServerSettings> ValidateStartupSettings(
        this OptionsBuilder<ServerSettings> builder,
        ISet<GameEra> enabledEras)
    {
        return builder
            .Validate(
                settings => HasExplicitGreenShopSetting(settings, enabledEras),
                "ServerSettings:Eras:Green:EnableShop is required when Green is enabled.")
            .Validate(
                settings => HasActiveGreenShopSeason(settings, enabledEras),
                "ServerSettings:Eras:Green:ActiveShopSeasonId must be a nonzero season id when Green EnableShop is true.");
    }

    private static bool HasExplicitGreenShopSetting(ServerSettings settings, ISet<GameEra> enabledEras)
        => !enabledEras.Contains(GameEra.Green)
           || settings.Eras.TryGetValue(nameof(GameEra.Green), out var greenSettings)
           && greenSettings.EnableShop.HasValue;

    private static bool HasActiveGreenShopSeason(ServerSettings settings, ISet<GameEra> enabledEras)
    {
        if (!enabledEras.Contains(GameEra.Green)
            || !settings.Eras.TryGetValue(nameof(GameEra.Green), out var greenSettings)
            || greenSettings.EnableShop != true)
        {
            return true;
        }

        return greenSettings.ActiveShopSeasonId is > 0;
    }
}
