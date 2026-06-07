using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Application.Catalog.Yellow;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public sealed class YellowRecommendLoader
{
    public const string FileName = "yellow_recommend_songs.json";

    public async Task<YellowRecommendEntry> LoadAsync(
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Yellow), FileName);
        var entry = await Ac15RecommendLoader.LoadFromFileAsync(path, catalogSongIds, cancellationToken);
        return Map(entry);
    }

    private static YellowRecommendEntry Map(Ac15RecommendEntry entry) => new()
    {
        RecommendSong = entry.RecommendSong,
        RecommendBestSongs = entry.RecommendBestSongs
    };
}
