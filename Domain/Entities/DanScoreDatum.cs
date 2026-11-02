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
    public List<DanStageScoreDatumNijiiro> DanStageScoreData { get; set; } = new();

    public virtual UserDatum? Ba { get; set; }
}