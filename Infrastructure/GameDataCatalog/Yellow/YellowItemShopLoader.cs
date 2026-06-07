using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Yellow;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public sealed class YellowItemShopLoader
{
    public const string FileName = "yellow_item_shop_data.json";

    public Task<YellowItemShopCatalog> LoadAsync(
        EraSettings yellowSettings,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Yellow), FileName);
        return LoadFromFileAsync(path, yellowSettings, cancellationToken);
    }

    public static async Task<YellowItemShopCatalog> LoadFromFileAsync(
        string path,
        EraSettings yellowSettings,
        CancellationToken cancellationToken)
    {
        var catalog = await Ac15ItemShopLoader.LoadFromFileAsync(
            path,
            yellowSettings.EnableShop == true,
            yellowSettings.ActiveShopSeasonId,
            nameof(GameEra.Yellow),
            cancellationToken);

        return Map(catalog);
    }

    private static YellowItemShopCatalog Map(Ac15ItemShopCatalog catalog)
    {
        if (!catalog.IsEnabled)
        {
            return YellowItemShopCatalog.Disabled;
        }

        return new YellowItemShopCatalog
        {
            IsEnabled = true,
            ActiveSeasonId = catalog.ActiveSeasonId,
            Seasons = catalog.Seasons.ToDictionary(
                pair => pair.Key,
                pair => new YellowItemShopSeason
                {
                    SeasonId = pair.Value.SeasonId,
                    VerupNo = pair.Value.VerupNo,
                    Telop = pair.Value.Telop,
                    StartDatetime = pair.Value.StartDatetime,
                    EndDatetime = pair.Value.EndDatetime,
                    AfterstartDays = pair.Value.AfterstartDays,
                    BeforecloseDays = pair.Value.BeforecloseDays,
                    Items = pair.Value.Items.Select(item => new YellowItemShopEntry
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
