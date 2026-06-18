using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenRecommendLoader
{
    public Task<Ac15RecommendEntry> LoadAsync(
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Green), "recommend_songs.json");
        return LoadFromFileAsync(path, catalogSongIds, cancellationToken);
    }

    public static Task<Ac15RecommendEntry> LoadFromFileAsync(
        string path,
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
        => Ac15RecommendLoader.LoadFromFileAsync(path, catalogSongIds, cancellationToken);
}
