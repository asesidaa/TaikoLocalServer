// ReSharper disable InconsistentNaming
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonPlayResultData
{
    public uint GetDonpoint { get; set; }

    public uint? RewardPtn { get; set; }

    public uint? RewardProgress { get; set; }

    public uint? DifficultyTutorialFlg { get; set; }
}
