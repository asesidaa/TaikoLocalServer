namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed class Ac15RecommendEntry
{
    public uint RecommendSong { get; init; }

    public IReadOnlyList<uint> RecommendBestSongs { get; init; } = [];

    public static Ac15RecommendEntry Empty { get; } = new();
}
