using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenMusicInfoEntry : IMusicInfoEntry
{
    public uint SongNo { get; init; }

    public string Title { get; init; } = string.Empty;

    public uint CategoryId { get; init; }
}
