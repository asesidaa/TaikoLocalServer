using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Domain.Enums;
using TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Yellow;

public sealed class YellowRecommendLoader
{
    public const string FileName = "yellow_recommend_songs.json";

    public Task<Ac15RecommendEntry> LoadAsync(
        IReadOnlySet<uint> catalogSongIds,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(PathHelper.GetDataPath(GameEra.Yellow), FileName);
        return Ac15RecommendLoader.LoadFromFileAsync(path, catalogSongIds, cancellationToken);
    }
}
