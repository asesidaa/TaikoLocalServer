using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenItemShopLoader
{
    public const string FileName = "green_item_shop_data.json";

    public Task<Ac15ItemShopCatalog> LoadAsync(
        EraSettings greenSettings,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Green), FileName);
        return LoadFromFileAsync(path, greenSettings, cancellationToken);
    }

    public static Task<Ac15ItemShopCatalog> LoadFromFileAsync(
        string path,
        EraSettings greenSettings,
        CancellationToken cancellationToken)
        => Ac15ItemShopLoader.LoadFromFileAsync(
            path,
            greenSettings.EnableShop == true,
            greenSettings.ActiveShopSeasonId,
            nameof(GameEra.Green),
            cancellationToken);
}
