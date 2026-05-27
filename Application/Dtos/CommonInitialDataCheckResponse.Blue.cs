namespace TaikoLocalServer.Application.Dtos;

public partial class CommonInitialDataCheckResponse
{
    public bool? IsBattleplay { get; set; }

    public byte[]? ReleaseBattleStageFlg { get; set; }

    public byte[]? ReleaseBattleSpecialFlg { get; set; }

    public uint? BattleBondsLvCap { get; set; }

    public List<InformationData> AryBlueTelopDatas { get; set; } = [];

    public List<InformationData> AryBlueEventFolderDatas { get; set; } = [];

    public List<InformationData> AryBlueTaikojukuDatas { get; set; } = [];

    public List<InformationData> AryBlueItemShopDatas { get; set; } = [];

    public List<InformationData> AryBlueLegaltermsDatas { get; set; } = [];
}
