using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Green;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenRecommendLoader
{
    public async Task<GreenRecommendEntry> LoadAsync(
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Green), "recommend_songs.json");
        return await LoadFromFileAsync(path, catalogSongIds, cancellationToken);
    }

    public static async Task<GreenRecommendEntry> LoadFromFileAsync(
        string path,
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        var entry = await Ac15RecommendLoader.LoadFromFileAsync(path, catalogSongIds, cancellationToken);
        return Map(entry);
    }

    private static GreenRecommendEntry Map(Ac15RecommendEntry entry) => new()
    {
        RecommendSong = entry.RecommendSong,
        RecommendBestSongs = entry.RecommendBestSongs
    };
}
