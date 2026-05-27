namespace TaikoLocalServer.Application.Catalog.Ac15;

public sealed class Ac15TaikojukuEntry
{
    public uint UniqueId { get; init; }

    public uint DanLevel { get; init; }

    public uint ChallengeLevel { get; init; }

    public string Name { get; init; } = string.Empty;

    public uint Difficulty { get; init; }

    public uint VerupNo { get; init; }

    public Ac15TaikojukuConditions Conditions { get; init; } = Ac15TaikojukuConditions.Empty;

    public Ac15TaikojukuConditions ExcellentConditions { get; init; } = Ac15TaikojukuConditions.Empty;

    public IReadOnlyList<Ac15TaikojukuSong> Songs { get; init; } = [];
}

public sealed class Ac15TaikojukuConditions
{
    public static Ac15TaikojukuConditions Empty { get; } = new();

    public uint SoulGauge { get; init; }

    public uint GoodCount { get; init; }

    public uint OkCount { get; init; }

    public uint BadCount { get; init; }

    public uint ComboCount { get; init; }

    public uint TotalHitCount { get; init; }

    public uint Score { get; init; }

    public uint DrumrollCount { get; init; }
}

public sealed class Ac15TaikojukuSong
{
    public string MusicId { get; init; } = string.Empty;

    public uint SongNo { get; init; }

    public uint Level { get; init; }

    public uint Notes { get; init; }
}
