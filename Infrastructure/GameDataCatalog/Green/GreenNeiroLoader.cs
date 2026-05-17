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
        var duplicateIds = items
            .GroupBy(neiro => neiro.NeiroId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(id => id)
            .ToArray();
        if (duplicateIds.Length > 0)
        {
            throw new InvalidDataException($"Duplicate Green neiro IDs in {path}: {string.Join(", ", duplicateIds)}");
        }

        return items
            .ToDictionary(neiro => neiro.NeiroId);
    }
}
