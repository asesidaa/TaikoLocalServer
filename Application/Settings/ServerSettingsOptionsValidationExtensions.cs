using Microsoft.Extensions.Options;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Settings;

public static class ServerSettingsOptionsValidationExtensions
{
    private static readonly GameEra[] Ac15ShopEras =
    [
        GameEra.Green,
        GameEra.Blue
    ];

    public static OptionsBuilder<ServerSettings> ValidateStartupSettings(
        this OptionsBuilder<ServerSettings> builder,
        ISet<GameEra> enabledEras)
    {
        foreach (var era in Ac15ShopEras)
        {
            builder = builder
                .Validate(
                    settings => HasExplicitShopSetting(settings, enabledEras, era),
                    $"ServerSettings:Eras:{era}:EnableShop is required when {era} is enabled.")
                .Validate(
                    settings => HasActiveShopSeason(settings, enabledEras, era),
                    $"ServerSettings:Eras:{era}:ActiveShopSeasonId must be a nonzero season id when {era} EnableShop is true.");
        }

        return builder;
    }

    private static bool HasExplicitShopSetting(ServerSettings settings, ISet<GameEra> enabledEras, GameEra era)
        => !enabledEras.Contains(era)
           || settings.Eras.TryGetValue(era.ToString(), out var eraSettings)
           && eraSettings.EnableShop.HasValue;

    private static bool HasActiveShopSeason(ServerSettings settings, ISet<GameEra> enabledEras, GameEra era)
    {
        if (!enabledEras.Contains(era)
            || !settings.Eras.TryGetValue(era.ToString(), out var eraSettings)
            || eraSettings.EnableShop != true)
        {
            return true;
        }

        return eraSettings.ActiveShopSeasonId is > 0;
    }
}
