using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTuningLoader
{
    public Task<IReadOnlyDictionary<string, Ac15StarSet>> LoadAsync(CancellationToken cancellationToken)
        => LoadFromFileAsync(GreenGameDataPaths.TuningBin, cancellationToken);

    public static Task<IReadOnlyDictionary<string, Ac15StarSet>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
        => Ac15TuningLoader.LoadFromFileAsync(path, nameof(GameEra.Green), cancellationToken);
}
