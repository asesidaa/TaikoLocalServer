using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenItemShopLoader
{
    public const string FileName = "green_item_shop_data.json";

    public Task<GreenItemShopCatalog> LoadAsync(
        EraSettings greenSettings,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Green), FileName);
        return LoadFromFileAsync(path, greenSettings, cancellationToken);
    }

    public static async Task<GreenItemShopCatalog> LoadFromFileAsync(
        string path,
        EraSettings greenSettings,
        CancellationToken cancellationToken)
    {
        var catalog = await Ac15ItemShopLoader.LoadFromFileAsync(
            path,
            greenSettings.EnableShop == true,
            greenSettings.ActiveShopSeasonId,
            nameof(GameEra.Green),
            cancellationToken);

        return Map(catalog);
    }

    private static GreenItemShopCatalog Map(Ac15ItemShopCatalog catalog)
    {
        if (!catalog.IsEnabled)
        {
            return GreenItemShopCatalog.Disabled;
        }

        return new GreenItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = catalog.ActiveSeasonId,
            Seasons = catalog.Seasons.ToDictionary(
                pair => pair.Key,
                pair => new GreenItemShopSeason
                {
                    SeasonId = pair.Value.SeasonId,
                    VerupNo = pair.Value.VerupNo,
                    Telop = pair.Value.Telop,
                    StartDatetime = pair.Value.StartDatetime,
                    EndDatetime = pair.Value.EndDatetime,
                    AfterstartDays = pair.Value.AfterstartDays,
                    BeforecloseDays = pair.Value.BeforecloseDays,
                    Items = pair.Value.Items.Select(item => new GreenItemShopEntry
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
