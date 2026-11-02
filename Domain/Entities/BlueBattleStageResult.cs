namespace TaikoLocalServer.Domain.Entities;

public sealed class BlueBattleStageResult
{
    public long Id { get; set; }

    public uint Baid { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PlayDatetime { get; set; }

    public uint? PlayMode { get; set; }

    public uint? StageMode { get; set; }

    public uint? StageIndex { get; set; }

    public uint? SongNo { get; set; }

    public uint? Level { get; set; }

    public uint? BattleStageId { get; set; }

    public uint? NpcId { get; set; }

    public uint? ResultType { get; set; }

    public uint? ClearFlag { get; set; }

    public uint? BossLife { get; set; }

    public uint? TotalExp { get; set; }

    public uint? AcquiredExp { get; set; }

    public uint? Dpn { get; set; }

    public uint? TokenId { get; set; }

    public uint? TokenValue { get; set; }

    public UserDatum? Ba { get; set; }
}
