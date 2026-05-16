namespace TaikoLocalServer.Application.Catalog.Green;

public sealed class GreenTaikojukuEntry
{
    public uint UniqueId { get; init; }

    public uint DanLevel { get; init; }

    public uint ChallengeLevel { get; init; }

    public string Name { get; init; } = string.Empty;

    public uint Difficulty { get; init; }

    public uint VerupNo { get; init; }

    public GreenTaikojukuConditions Conditions { get; init; } = GreenTaikojukuConditions.Empty;

    public GreenTaikojukuConditions ExcellentConditions { get; init; } = GreenTaikojukuConditions.Empty;

    public IReadOnlyList<GreenTaikojukuSong> Songs { get; init; } = [];
}

public sealed class GreenTaikojukuConditions
{
    public static GreenTaikojukuConditions Empty { get; } = new();

    public uint SoulGauge { get; init; }

    public uint GoodCount { get; init; }

    public uint OkCount { get; init; }

    public uint BadCount { get; init; }

    public uint ComboCount { get; init; }

    public uint TotalHitCount { get; init; }

    public uint Score { get; init; }

    public uint DrumrollCount { get; init; }
}

public sealed class GreenTaikojukuSong
{
    public string MusicId { get; init; } = string.Empty;

    public uint SongNo { get; init; }

    public uint Level { get; init; }

    public uint Notes { get; init; }
}
