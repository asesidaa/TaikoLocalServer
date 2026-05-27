using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueItemShopLoader
{
    public const string FileName = "blue_item_shop_data.json";

    public async Task<BlueItemShopCatalog> LoadAsync(
        EraSettings blueSettings,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Blue), FileName);
        var catalog = await Ac15ItemShopLoader.LoadFromFileAsync(
            path,
            blueSettings.EnableShop == true,
            blueSettings.ActiveShopSeasonId,
            nameof(GameEra.Blue),
            cancellationToken);

        return Map(catalog);
    }

    private static BlueItemShopCatalog Map(Ac15ItemShopCatalog catalog)
    {
        if (!catalog.IsEnabled)
        {
            return BlueItemShopCatalog.Disabled;
        }

        return new BlueItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = catalog.ActiveSeasonId,
            Seasons = catalog.Seasons.ToDictionary(
                pair => pair.Key,
                pair => new BlueItemShopSeason
                {
                    SeasonId = pair.Value.SeasonId,
                    VerupNo = pair.Value.VerupNo,
                    Telop = pair.Value.Telop,
                    StartDatetime = pair.Value.StartDatetime,
                    EndDatetime = pair.Value.EndDatetime,
                    AfterstartDays = pair.Value.AfterstartDays,
                    BeforecloseDays = pair.Value.BeforecloseDays,
                    Items = pair.Value.Items.Select(item => new BlueItemShopEntry
                    {
                        ItemNo = item.ItemNo,
                        ItemType = item.ItemType,
                        ItemId = item.ItemId,
                        Price = item.Price
                    }).ToArray()
                })
        };
    }
}
