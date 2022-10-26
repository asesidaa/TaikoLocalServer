namespace GameDatabase.Entities;

public class DanStageScoreDatum
{
    public uint Baid { get; set; }

    public uint DanId { get; set; }

    public uint SongNumber { get; set; }

    public uint PlayScore { get; set; }

    public uint GoodCount { get; set; }

    public uint OkCount { get; set; }

    public uint BadCount { get; set; }

    public uint DrumrollCount { get; set; }

    public uint TotalHitCount { get; set; }

    public uint ComboCount { get; set; }

    public uint HighScore { get; set; }

    public DanScoreDatum Parent { get; set; } = null!;
}