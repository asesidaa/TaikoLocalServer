using SharedProject.Enums;

namespace SharedProject.Models;

public class DanBestData
{
    public uint DanId { get; set; }

    public DanClearState ClearState { get; set; }

    public uint SoulGaugeTotal { get; set; }

    public uint ComboCountTotal { get; set; }

    public List<DanBestStageData> DanBestStageDataList { get; set; } = new();
}