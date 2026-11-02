using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Abstractions;

/// <summary>The live item shop season per game, kept in the database; the catalogs read it when they load.</summary>
public interface IShopLiveSeasons
{
    Task InitializeAsync();

    /// <summary>The season an admin made live, or null (then the config's ActiveShopSeasonId).</summary>
    uint? Get(GameEra era);

    Task SetAsync(GameEra era, uint seasonId, CancellationToken cancellationToken);

    /// <summary>The game's settings with the live season in place of the config's.</summary>
    EraSettings Apply(GameEra era, EraSettings settings) => Get(era) is not { } id ? settings : new EraSettings
    {
        Enabled = settings.Enabled, AutoExtractCatalog = settings.AutoExtractCatalog, GameDataPath = settings.GameDataPath,
        CustomizationNameDataPath = settings.CustomizationNameDataPath, EnableShop = settings.EnableShop, ActiveShopSeasonId = id,
        EnableDonChallenge = settings.EnableDonChallenge, ActiveDonChallengeBundleId = settings.ActiveDonChallengeBundleId,
    };
}
