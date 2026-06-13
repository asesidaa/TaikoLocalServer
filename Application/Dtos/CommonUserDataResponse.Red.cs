namespace TaikoLocalServer.Application.Dtos;

public partial class CommonUserDataResponse
{
    public bool? IsDevilRed { get; set; }

    public bool? IsExplainRed { get; set; }

    public uint? TotalGetDonpoint { get; set; }

    public uint? TotalUseDonpoint { get; set; }

    public uint? RewardProgress { get; set; }

    public uint? DifficultyTutorialFlg { get; set; }
}
