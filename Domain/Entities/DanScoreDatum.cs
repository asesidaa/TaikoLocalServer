using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public class DanScoreDatumNijiiro
{
    public uint Baid { get; set; }
    public uint DanId { get; set; }
    public DanType DanType { get; set; }
    public uint ArrivalSongCount { get; set; }
    public uint SoulGaugeTotal { get; set; }
    public uint ComboCountTotal { get; set; }
    public DanClearState ClearState { get; set; }
    /// <summary>Runs counted since this was added (older bests start at 0).</summary>
    public uint PlayCount { get; set; }
    /// <summary>The run that set the best: the highest pass, then the highest total score at it. Null for bests
    /// from before this was recorded.</summary>
    public DateTime? BestRunAt { get; set; }
    public List<DanStageScoreDatumNijiiro> DanStageScoreData { get; set; } = new();

    public virtual UserDatum? Ba { get; set; }
}