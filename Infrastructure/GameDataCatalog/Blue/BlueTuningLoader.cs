using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueTuningLoader
{
    public Task<IReadOnlyDictionary<string, Ac15StarSet>> LoadAsync(CancellationToken cancellationToken)
    {
        return LoadFromFileAsync(BlueGameDataPaths.TuningBin, cancellationToken);
    }

    public static Task<IReadOnlyDictionary<string, Ac15StarSet>> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
        => Ac15TuningLoader.LoadFromFileAsync(path, nameof(GameEra.Blue), cancellationToken);
}
