using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueCostumeLoader
{
    public const string FileName = "blue_costume_data.json";

    public Task<IReadOnlyList<Costume>> LoadAsync(CancellationToken cancellationToken)
        => Ac15CustomizationCatalogLoader.LoadListAsync<Costume>(
            Path.Combine(PathHelper.GetDataPath(GameEra.Blue), FileName),
            cancellationToken);
}
