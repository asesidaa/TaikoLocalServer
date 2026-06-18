using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenMusicInfoLoader
{
    public Task<Ac15MusicInfoLoadResult> LoadAsync(CancellationToken cancellationToken)
        => LoadFromFileAsync(GreenGameDataPaths.MusicInfoXml, cancellationToken);

    public static Task<Ac15MusicInfoLoadResult> LoadFromFileAsync(
        string path,
        CancellationToken cancellationToken)
        => Ac15MusicInfoLoader.LoadFromFileAsync(path, cancellationToken);
}
