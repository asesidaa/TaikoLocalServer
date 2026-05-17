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
        return items
            .GroupBy(title => title.TitleId)
            .ToDictionary(group => group.Key, group => group.First());
    }
}
