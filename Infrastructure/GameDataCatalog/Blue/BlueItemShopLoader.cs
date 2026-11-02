using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueItemShopLoader
{
    public const string FileName = "blue_item_shop_data.json";

    public Task<Ac15ItemShopCatalog> LoadAsync(
        EraSettings blueSettings,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Blue), FileName);
        return Ac15ItemShopLoader.LoadFromFileAsync(
            path,
            blueSettings.EnableShop == true,
            blueSettings.ActiveShopSeasonId,
            nameof(GameEra.Blue),
            cancellationToken);
    }
}
