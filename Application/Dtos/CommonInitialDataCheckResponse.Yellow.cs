namespace TaikoLocalServer.Application.Dtos;

public partial class CommonInitialDataCheckResponse
{
    public List<InformationData> AryYellowTelopDatas { get; set; } = [];

    public List<InformationData> AryYellowEventFolderDatas { get; set; } = [];

    public List<InformationData> AryYellowTaikojukuDatas { get; set; } = [];

    public List<InformationData> AryYellowItemShopDatas { get; set; } = [];

    public List<InformationData> AryYellowLegaltermsDatas { get; set; } = [];
}
