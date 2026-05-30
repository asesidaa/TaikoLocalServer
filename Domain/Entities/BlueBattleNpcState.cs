namespace TaikoLocalServer.Domain.Entities;

public sealed class BlueBattleNpcState
{
    public uint Baid { get; set; }

    public uint NpcId { get; set; }

    public uint? TotalExp { get; set; }

    public uint? MaxDaniPower { get; set; }

    public byte[]? NpcCostumeFlg { get; set; }

    public uint? SelectedSpecialId { get; set; }

    public byte[]? ReleaseSpecialFlg { get; set; }

    public uint? BondsLevel { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public UserDatum? Ba { get; set; }
}
