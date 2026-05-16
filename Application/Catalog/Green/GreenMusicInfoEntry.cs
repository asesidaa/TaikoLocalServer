using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Application.Catalog.Green;

public sealed record class GreenMusicInfoEntry : IMusicInfoEntry
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

    public uint CategoryId { get; init; }

    public uint DemoPlay { get; init; }

    public IReadOnlyList<uint> Tags { get; init; } = [];

    public int FileOrder { get; init; }

    public uint StarEasy { get; init; }

    public uint StarNormal { get; init; }

    public uint StarHard { get; init; }

    public uint StarOni { get; init; }

    public uint StarUra { get; init; }
}
