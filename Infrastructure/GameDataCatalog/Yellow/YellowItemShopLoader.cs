using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public sealed class YellowItemShopLoader
{
    public const string FileName = "yellow_item_shop_data.json";

    public Task<Ac15ItemShopCatalog> LoadAsync(
        EraSettings yellowSettings,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Yellow), FileName);
        return LoadFromFileAsync(path, yellowSettings, cancellationToken);
    }

    public static Task<Ac15ItemShopCatalog> LoadFromFileAsync(
        string path,
        EraSettings yellowSettings,
        CancellationToken cancellationToken)
        => Ac15ItemShopLoader.LoadFromFileAsync(
            path,
            yellowSettings.EnableShop == true,
            yellowSettings.ActiveShopSeasonId,
            nameof(GameEra.Yellow),
            cancellationToken);
}
