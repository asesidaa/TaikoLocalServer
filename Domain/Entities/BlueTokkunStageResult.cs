namespace TaikoLocalServer.Domain.Entities;

public sealed class BlueTokkunStageResult
{
    public long Id { get; set; }

    public uint Baid { get; set; }

    public string PlayDatetime { get; set; } = string.Empty;

    public uint PlayMode { get; set; }

    public string BanacoinDatetime { get; set; } = string.Empty;

    public uint TokkunSongCnt { get; set; }

    public string TookunSongnoesJson { get; set; } = string.Empty;

    public uint TokkunSpeedchangeCnt { get; set; }

    public uint TokkunAutoplayCnt { get; set; }

    public uint TokkunJumpCnt { get; set; }

    public UserDatum? Ba { get; set; }
}
