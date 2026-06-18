using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueMusicInfoLoader
{
    public Task<Ac15MusicInfoLoadResult> LoadAsync(CancellationToken cancellationToken)
        => LoadFromFileAsync(BlueGameDataPaths.MusicInfoXml, cancellationToken);

    public static Task<Ac15MusicInfoLoadResult> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
        => Ac15MusicInfoLoader.LoadFromFileAsync(path, cancellationToken);
}
