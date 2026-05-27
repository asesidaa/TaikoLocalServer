using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Blue;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Blue;

public sealed class BlueRecommendLoader
{
    public const string FileName = "blue_recommend_songs.json";

    public async Task<BlueRecommendEntry> LoadAsync(
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Blue), FileName);
        var entry = await Ac15RecommendLoader.LoadFromFileAsync(path, catalogSongIds, cancellationToken);
        return Map(entry);
    }

    private static BlueRecommendEntry Map(Ac15RecommendEntry entry) => new()
    {
        RecommendSong = entry.RecommendSong,
        RecommendBestSongs = entry.RecommendBestSongs
    };
}
