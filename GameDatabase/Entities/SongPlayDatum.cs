using SharedProject.Enums;

namespace GameDatabase.Entities;

public class SongPlayDatum
{
    public long Id { get; set; }
    public uint Baid { get; set; }

    public uint SongNumber { get; set; }

    public uint SongId { get; set; }
    public Difficulty Difficulty { get; set; }
    public CrownType Crown { get; set; }
    public uint Score { get; set; }
    public uint ScoreRate { get; set; }
    public ScoreRank ScoreRank { get; set; }
    public uint GoodCount { get; set; }
    public uint OkCount { get; set; }
    public uint MissCount { get; set; }
    public uint ComboCount { get; set; }
    public uint HitCount { get; set; }
    public uint DrumrollCount { get; set; }
    public bool Skipped { get; set; }
    public DateTime PlayTime { get; set; }
    public short Option { get; set; }
    public virtual Card? Ba { get; set; }
}