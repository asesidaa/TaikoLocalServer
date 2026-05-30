namespace TaikoLocalServer.Domain.Entities;

public sealed class BlueBattleUserState
{
    public uint Baid { get; set; }

    public byte[]? ReleaseInfoFlg { get; set; }

    public byte[]? ReleaseBattleStageFlg { get; set; }

    public uint? LastBattleStageId { get; set; }

    public uint? LastBossLife { get; set; }

    public uint? LastNpcId { get; set; }

    public uint? AssignStageId { get; set; }

    public uint? BattleBondsLvCap { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public UserDatum? Ba { get; set; }
}
