namespace TaikoLocalServer.Application.Catalog.Yellow;

public sealed class YellowRecommendEntry
{
    public uint RecommendSong { get; init; }

    public IReadOnlyList<uint> RecommendBestSongs { get; init; } = [];

    public static YellowRecommendEntry Empty { get; } = new();
}
