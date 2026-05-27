using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public class DanScoreDatumBlue
{
    public uint Baid { get; set; }
    public uint DanId { get; set; }
    public bool IsExtra { get; set; }
    public uint MedleyUniqueId { get; set; }
    public uint ArrivalSongCount { get; set; }
    public uint SoulGaugeTotal { get; set; }
    public uint ComboCountTotal { get; set; }
    public BlueDanClearGrade ClearGrade { get; set; }
    public List<DanStageScoreDatumBlue> DanStageScoreData { get; set; } = [];

    public virtual UserDatum? Ba { get; set; }
}
