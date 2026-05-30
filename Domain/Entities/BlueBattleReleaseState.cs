namespace TaikoLocalServer.Domain.Entities;

public sealed class BlueBattleReleaseState
{
    public long Id { get; set; }

    public uint Baid { get; set; }

    public uint? ReleaseInfoId { get; set; }

    public uint? ReleaseBattleStageId { get; set; }

    public uint? ReleaseNpcId { get; set; }

    public uint? ReleaseNpcCostumeId { get; set; }

    public uint? ReleaseNpcSpecialId { get; set; }

    public uint? AssignNextStageId { get; set; }

    public uint? TokenId { get; set; }

    public uint? TokenValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public UserDatum? Ba { get; set; }
}
