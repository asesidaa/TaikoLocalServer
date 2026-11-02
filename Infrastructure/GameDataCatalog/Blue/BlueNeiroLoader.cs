using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueNeiroLoader
{
    public const string FileName = "blue_neiro_data.json";

    public async Task<IReadOnlyDictionary<uint, Neiro>> LoadAsync(CancellationToken cancellationToken)
    {
        var items = await Ac15CustomizationCatalogLoader.LoadListAsync<Neiro>(
            Path.Combine(PathHelper.GetDataPath(GameEra.Blue), FileName),
            cancellationToken);
        return items
            .GroupBy(neiro => neiro.NeiroId)
            .ToDictionary(group => group.Key, group => group.First());
    }
}
