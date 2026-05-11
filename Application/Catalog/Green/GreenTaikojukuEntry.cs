namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenTaikojukuEntry
{
    public uint DanLevel { get; init; }

    public IReadOnlyList<GreenTaikojukuSong> Songs { get; init; } = [];
}

public sealed class GreenTaikojukuSong
{
    public uint SongNo { get; init; }

    public uint Level { get; init; }
}
