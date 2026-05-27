namespace TaikoLocalServer.Application.Catalog.Blue;

public sealed class BlueRecommendEntry
{
    public uint RecommendSong { get; init; }

    public IReadOnlyList<uint> RecommendBestSongs { get; init; } = [];

    public static BlueRecommendEntry Empty { get; } = new();
}
