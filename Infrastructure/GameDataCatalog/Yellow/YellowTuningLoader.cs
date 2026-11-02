using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public sealed class YellowTuningLoader
{
    public Task<IReadOnlyDictionary<string, Ac15StarSet>> LoadAsync(CancellationToken cancellationToken)
        => LoadFromFileAsync(YellowGameDataPaths.TuningBin, cancellationToken);

    public static Task<IReadOnlyDictionary<string, Ac15StarSet>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
        => Ac15TuningLoader.LoadFromFileAsync(path, nameof(GameEra.Yellow), cancellationToken);
}
