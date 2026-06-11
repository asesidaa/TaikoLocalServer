namespace TaikoLocalServer.Application.Dtos;

public partial class CommonInitialDataCheckResponse
{
    public bool? IsBattleplay { get; set; }

    public byte[]? ReleaseBattleStageFlg { get; set; }

    public byte[]? ReleaseBattleSpecialFlg { get; set; }

    public uint? BattleBondsLvCap { get; set; }

}
