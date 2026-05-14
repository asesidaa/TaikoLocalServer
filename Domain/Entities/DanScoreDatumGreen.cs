using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public class DanScoreDatumGreen
{
    public uint Baid { get; set; }
    public uint DanId { get; set; }
    public bool IsExtra { get; set; }
    public uint MedleyUniqueId { get; set; }
    public uint ArrivalSongCount { get; set; }
    public uint SoulGaugeTotal { get; set; }
    public uint ComboCountTotal { get; set; }
    public GreenDanClearGrade ClearGrade { get; set; }
    public List<DanStageScoreDatumGreen> DanStageScoreData { get; set; } = [];

    public virtual UserDatum? Ba { get; set; }
}
