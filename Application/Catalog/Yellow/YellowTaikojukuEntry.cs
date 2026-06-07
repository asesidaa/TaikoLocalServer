namespace TaikoLocalServer.Application.Catalog.Yellow;

public sealed class YellowTaikojukuEntry
{
    public uint UniqueId { get; init; }

    public uint DanLevel { get; init; }

    public uint ChallengeLevel { get; init; }

    public string Name { get; init; } = string.Empty;

    public uint Difficulty { get; init; }

    public uint VerupNo { get; init; }

    public YellowTaikojukuConditions Conditions { get; init; } = YellowTaikojukuConditions.Empty;

    public YellowTaikojukuConditions ExcellentConditions { get; init; } = YellowTaikojukuConditions.Empty;

    public IReadOnlyList<YellowTaikojukuSong> Songs { get; init; } = [];
}

public sealed class YellowTaikojukuConditions
{
    public static YellowTaikojukuConditions Empty { get; } = new();

    public uint SoulGauge { get; init; }

    public uint GoodCount { get; init; }

    public uint OkCount { get; init; }

    public uint BadCount { get; init; }

    public uint ComboCount { get; init; }

    public uint TotalHitCount { get; init; }

    public uint Score { get; init; }

    public uint DrumrollCount { get; init; }
}

public sealed class YellowTaikojukuSong
{
    public string MusicId { get; init; } = string.Empty;

    public uint SongNo { get; init; }

    public uint Level { get; init; }

    public uint Notes { get; init; }
}
