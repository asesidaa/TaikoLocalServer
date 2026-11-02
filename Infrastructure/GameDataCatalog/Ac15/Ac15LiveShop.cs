using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

/// <summary>Loads a game's item shop with the live season an admin picked (IShopLiveSeasons) in place of the config's.</summary>
public static class Ac15LiveShop
{
    public static async Task<Ac15ItemShopCatalog> LoadAsync(
        GameEra era, EraSettings settings, IShopLiveSeasons? live, Func<EraSettings, Task<Ac15ItemShopCatalog>> load)
    {
        var withLive = live?.Apply(era, settings) ?? settings;
        if (ReferenceEquals(withLive, settings)) return await load(settings);
        try
        {
            return await load(withLive);
        }
        catch (InvalidDataException)
        {
            // The live season isn't in the file any more (deleted in Game data): the config's season instead.
            return await load(settings);
        }
    }
}
