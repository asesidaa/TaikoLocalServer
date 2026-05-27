namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed record class Ac15MusicInfoEntry
{
    public string MusicId { get; init; } = string.Empty;

    public uint SongNo { get; init; }

    public uint NewRelease { get; init; }

    public bool IsSecret { get; init; }

    public bool IsPapaMama { get; init; }

    public bool HasExtreme { get; init; }

    public string PartsSet { get; init; } = string.Empty;

    public string WaiwaiPartsSet { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string GenreName { get; init; } = string.Empty;

    public uint DemoPlay { get; init; }

    public IReadOnlyList<uint> Tags { get; init; } = [];

    public int FileOrder { get; init; }
}
