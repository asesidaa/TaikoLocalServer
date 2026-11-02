using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenNeiroLoader
{
    public async Task<IReadOnlyDictionary<uint, Neiro>> LoadAsync(CancellationToken cancellationToken)
        => await LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Green), GreenCatalogExtractor.NeiroFileName),
            cancellationToken);

    public static async Task<IReadOnlyDictionary<uint, Neiro>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var items = await GreenCustomizationCatalogLoader.LoadListAsync<Neiro>(path, cancellationToken);
        return items
            .GroupBy(neiro => neiro.NeiroId)
            .ToDictionary(group => group.Key, group => group.First());
    }
}
