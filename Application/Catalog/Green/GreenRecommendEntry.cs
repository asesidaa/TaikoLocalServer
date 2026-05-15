namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenRecommendEntry
{
    public uint RecommendSong { get; init; }

    public IReadOnlyList<uint> RecommendBestSongs { get; init; } = [];

    public static GreenRecommendEntry Empty { get; } = new();
}
