using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTitleLoader
{
    public async Task<IReadOnlyDictionary<uint, Title>> LoadAsync(CancellationToken cancellationToken)
        => await LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Green), GreenCatalogExtractor.TitleFileName),
            cancellationToken);

    public static async Task<IReadOnlyDictionary<uint, Title>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var items = await GreenCustomizationCatalogLoader.LoadListAsync<Title>(path, cancellationToken);
        var duplicateIds = items
            .GroupBy(title => title.TitleId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(id => id)
            .ToArray();
        if (duplicateIds.Length > 0)
        {
            throw new InvalidDataException($"Duplicate Green title IDs in {path}: {string.Join(", ", duplicateIds)}");
        }

        return items
            .ToDictionary(title => title.TitleId);
    }
}
