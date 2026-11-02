using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public class DanScoreDatumRed : IAc15DanScoreDatum
{
    public uint Baid { get; set; }
    public uint DanId { get; set; }
    public bool IsExtra { get; set; }
    public uint MedleyUniqueId { get; set; }
    public uint ArrivalSongCount { get; set; }
    public uint SoulGaugeTotal { get; set; }
    public uint ComboCountTotal { get; set; }
    public Ac15DanClearGrade ClearGrade { get; set; }
    public List<DanStageScoreDatumRed> DanStageScoreData { get; set; } = [];

    public virtual UserDatum? Ba { get; set; }
}
