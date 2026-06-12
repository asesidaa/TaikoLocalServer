using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed record class Ac15MusicInfoEntry : IMusicInfoEntry
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

    public byte StarEasy { get; init; }

    public byte StarNormal { get; init; }

    public byte StarHard { get; init; }

    public byte StarOni { get; init; }

    public byte StarUra { get; init; }
}
