using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Green.Extractor;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenCostumeLoader
{
    public Task<IReadOnlyList<Costume>> LoadAsync(CancellationToken cancellationToken)
        => LoadFromFileAsync(
            Path.Combine(PathHelper.GetDataPath(GameEra.Green), GreenCatalogExtractor.CostumeFileName),
            cancellationToken);

    public static Task<IReadOnlyList<Costume>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
        => GreenCustomizationCatalogLoader.LoadListAsync<Costume>(path, cancellationToken);
}
